import json
from random import random
import os

AMOUNT = 100
COEFFICIENT_SET_LENGTH = 20
MIN_DIMENSION = 2
MAX_DIMENSION = 5
JSON_PATH = os.path.join(os.path.dirname(__file__),  './matrices.json')

matrices = []

def get_vector(dimension):
    b = []
    for i in range(dimension):
        b += [random()*COEFFICIENT_SET_LENGTH - COEFFICIENT_SET_LENGTH/2]

    return b

def get_matrix_with_appended_m_identity(m, n):
    mat = []
    for r in range(m):
        row = get_vector(n)
        row += get_canonical(m, r)
        mat += row
    
    return mat

def get_canonical(dim, i):
    return [1 if j == i else 0 for j in range(dim)]

for i in range(AMOUNT):
    m = int(random()*(MAX_DIMENSION - MIN_DIMENSION) + MIN_DIMENSION)
    n = int(random()*(MAX_DIMENSION - MIN_DIMENSION) + MIN_DIMENSION)

    a = get_matrix_with_appended_m_identity(m, n)
    b = get_vector(m)
    c = get_vector(n) + [0 for i in range(m)]

    matrices.append({
        "A": a,
        "Am": m,
        "An": n + m,
        "b": b,
        "c": c
    })

with open(JSON_PATH, 'w') as f:
    json.dump(matrices, f)
