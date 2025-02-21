from io import BufferedRandom
import json
from os import terminal_size
import re
from enum import Enum
import subprocess

class Status(Enum):
    ParsingObjective = 1
    ParsingRestrictions = 2
    ParsingSignRestrictions = 3

class Term:
    def __init__(self, var_name: str, coeff: float, end: int):
        self.var_name = var_name
        self.coeff = coeff
        self.end = end

class Expression:
    def __init__(self, variable_coeffs: dict[str, float], end: int):
        self.variable_coeffs = variable_coeffs
        self.end = end

class ParseletReturn:
    def __init__(self, string, end: int) -> None:
        self.value = string
        self.end = end

class Restriction:
    def __init__(self, expression: Expression, sign: int, bound: float) -> None:
        self.expression = expression
        self.sign = sign
        self.bound = bound

class SignRestriction:
    def __init__(self, var_name: str):
        self.var_name = var_name

def parse_variable(line: str, pos: int) -> ParseletReturn:
    match = re.match(r'[A-Za-z][A-Za-z0-9]*', line[pos:])
    if not match:
        raise Exception(f"Invalid variable at {pos}")
    return ParseletReturn(match.group(), pos + match.group().__len__())

def parse_term(line: str, pos: int) -> Term:
    # print('parsing term ', line[pos:])
    match = re.match(r'([+-]?[0-9]*(\.[0-9]+)?)?\*?', line[pos:])
    if not match or len(match.group()) == 0 or match.group() == '+':
        coeff = 1
    elif match.group() == '-':
        coeff = -1
    else:
        coeff = float(match.groups()[0])
    var_parselet_result = parse_variable(line, pos + len(match.group()) if match else pos)
    return Term(var_parselet_result.value, coeff, var_parselet_result.end)

def eat_WS(line: str, pos: int) -> int:
    while(pos < len(line) and re.match(r'\s', line[pos])):
        pos += 1
    return pos

def parse_expression(line: str, i: int) -> Expression:
    # print('parsing expression')
    variable_coeffs: dict[str, float] = {}
    i = eat_WS(line, i)
    term = parse_term(line, i)
    i = term.end
    i = eat_WS(line, i)
    if term.coeff != 0:
        variable_coeffs[term.var_name] = term.coeff
    while i < len(line):
        # print('iteration i = ', i)

        sign = re.match(r'[+-]', line[i:])
        if not sign:
            break
        i += len(sign.group())
        i = eat_WS(line, i)
        term = parse_term(line, i)
        i = term.end
        i = eat_WS(line, i)
        if sign.group() == '-':
            term.coeff*=-1

        if term.coeff == 0: continue
        if term.var_name not in variable_coeffs:
            variable_coeffs[term.var_name] = 0
        variable_coeffs[term.var_name] += term.coeff
        if variable_coeffs[term.var_name] == 0:
            variable_coeffs.__delattr__(term.var_name)


        i = eat_WS(line, i)
    return Expression(variable_coeffs, i)

def parse_objective(line: str) -> Expression: 
    max_min_match = re.match(r'\s*(max|min)', line)
    if not max_min_match:
        raise Exception('Objective function must start with "max" or "min"')
    
    # print('max min parsed')

    i = eat_WS(line, max_min_match.end())
    max_min = max_min_match.group().strip()

    objective = parse_expression(line, i)

    # print('expression parsed')

    if max_min == 'max':
        for key, val in objective.variable_coeffs.items():
            objective.variable_coeffs[key] = -val

    return objective

def parse_ineq_sign(line: str, i: int) -> ParseletReturn:
    sign_match = re.match(r'([<>]=?|=)', line[i:])
    if not sign_match:
        raise Exception(f"Inequality or equality sign expected at {i}")
    
    if sign_match.group().startswith('<'):
        sign = 1
    elif sign_match.group().startswith('>'):
        sign = -1
    else:
        sign = 0

    return ParseletReturn(sign, i + len(sign_match.group()))


def parse_restriction(line: str) -> Restriction:
    expr = parse_expression(line, 0)
    i = expr.end
    i = eat_WS(line, i)
    sign_parselet_return = parse_ineq_sign(line, i) 
    try:
        bound = float(line[sign_parselet_return.end:].strip())
        # print('restriction parsed')
        # print(expr.variable_coeffs)
        return Restriction(expr, sign_parselet_return.value, bound)
    except:
        raise Exception(F"Bound expected at {sign_parselet_return.end}")


def parse_sign_restriction(line: str) -> SignRestriction:
    i = 0
    i = eat_WS(line, i)
    var_match = parse_variable(line, i)
    if not var_match:
        raise Exception(f'Variable expected at {i}')

    var_name = var_match.value
    i = var_match.end

    i = eat_WS(line, i)

    sign_parselet_return = parse_ineq_sign(line, i)
    i = sign_parselet_return.end
    # print('Parsing ineq restriction: ', line[i:])
    
    rest = line[i:].strip()

    if rest != '0' or sign_parselet_return.value >= 0:
        raise Exception(">= 0 restriction expected")


    return SignRestriction(var_name)



def main():
    # print("Enter your LPP problem line by line. Press Enter twice to finish input.")
    variables = {}
    
    line = input('objective function > ')
    objective_fn = parse_objective(line)

    restrictions: list[Restriction] = []
    while True:
        line = input('restriction > ')
        if line.strip() == '':
            break
        
        # print('parsing restriction')

        restrictions += [parse_restriction(line)]

    sign_restricted_vars = set()
    while True:
        line = input('sign restriction (VAR >= 0) > ')
        if line.strip() == '':
            break

        # print('parsing sign restriction')

        sign_restricted_vars.add(parse_sign_restriction(line).var_name)

    vars = set(objective_fn.variable_coeffs.keys())

    for restriction in restrictions:
        for var_name in restriction.expression.variable_coeffs.keys():
            vars.add(var_name)

    for sign_restricted_var in sign_restricted_vars:
        vars.add(sign_restricted_var)

    vars_list = list(vars)
    vars_list.sort()

    matrix: list[list[float]] = []
    for restriction in restrictions:
        matrix_row: list[float] = []
        for var in vars_list:
            if var in restriction.expression.variable_coeffs:
                matrix_row += [restriction.expression.variable_coeffs[var]]
            else:
                matrix_row += [0]
        matrix += [matrix_row]

    vectorM: list[float] = []
    for restriction in restrictions:
        vectorM += [restriction.bound]

    vectorN: list[float] = []
    for var_name in vars_list:
        if var_name in objective_fn.variable_coeffs:
            vectorN += [objective_fn.variable_coeffs[var_name]]
        else:
            vectorN += [0]

    binaryVectorN: list[int] = []
    for var_name in vars_list:
        if var_name in sign_restricted_vars:
            binaryVectorN += [1]
        else:
            binaryVectorN += [0]

    vectorSign: list[int] = []
    for restriction in restrictions:
        vectorSign += [restriction.sign]

    with open('problem.json', 'w') as f:
        f.write(json.dumps({
            'matrix': matrix,
            'vectorM': vectorM,
            'vectorN': vectorN,
            'vectorSign': vectorSign,
            'binaryVectorN': binaryVectorN,
        }, indent=2))

    



if __name__ == "__main__":
    main()

