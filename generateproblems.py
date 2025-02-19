import json
import random
from scipy.optimize import linprog

def generate_random_problem():
    while True:
        n = random.randint(1, 5)  # Random number of variables
        m = random.randint(1, n)  # Ensure m <= n
        
        vectorN = [random.randint(-10, 10) for _ in range(n)]
        matrix = [[random.randint(-10, 10) for _ in range(n)] for _ in range(m)]
        vectorM = [random.randint(1, 20) for _ in range(m)]
        vectorSign = [random.choice([1]) for _ in range(m)]
        binaryVectorN = [random.choice([1]) for _ in range(n)]
        
        problem_data = {
            "vectorN": vectorN,
            "matrix": matrix,
            "vectorM": vectorM,
            "vectorSign": vectorSign,
            "binaryVectorN": binaryVectorN
        }
        
        solution = solve_simplex(problem_data)
        if solution["success"]:
            return problem_data, solution

def solve_simplex(problem_data):
    c = problem_data["vectorN"]  # Coefficients of the objective function
    A = problem_data["matrix"]  # Constraint coefficients
    b = problem_data["vectorM"]  # Right-hand side values of constraints
    signs = problem_data["vectorSign"]  # Constraint signs

    # Adjusting constraints based on sign (1 for <=, 0 for =, -1 for >=)
    A_ub, b_ub, A_eq, b_eq = [], [], [], []
    for i, sign in enumerate(signs):
        if sign == 1:
            A_ub.append(A[i])
            b_ub.append(b[i])
        elif sign == 0:
            A_eq.append(A[i])
            b_eq.append(b[i])
        elif sign == -1:
            A_ub.append([-x for x in A[i]])
            b_ub.append(-b[i])

    res = linprog(c=c, A_ub=A_ub if A_ub else None, b_ub=b_ub if b_ub else None,
                  A_eq=A_eq if A_eq else None, b_eq=b_eq if b_eq else None, method="highs")

    solution = {
        "solution_vector": res.x.tolist() if res.success else None,
        "evaluation_value": res.fun if res.success else None,
        "success": res.success,
        "message": res.message
    }
    return solution

# Generate and solve random feasible problems
problems_solutions = [generate_random_problem() for _ in range(5)]
output_data = [{"problem": p, "solution": s} for p, s in problems_solutions]

# Save the problems and solutions in a JSON file
with open("random_solutions.json", "w") as f:
    json.dump(output_data, f, indent=4)

print("Random feasible problems and solutions saved in random_solutions.json")