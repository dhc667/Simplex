import sys
import json
import scipy
import scipy.linalg
import scipy.optimize
import time

if len(sys.argv) != 2:
    print("Usage: python solvePOL.py '<json_string>'")
    sys.exit(1)

try:
    data = json.loads(sys.argv[1])
    a: list[list[float]] = data["a"]
    b: list[float] = data["b"]
    c: list[float] = data["c"]
except (json.JSONDecodeError, KeyError) as e:
    print(f"Invalid input: {e}")
    sys.exit(1)


t0 = time.time()
s = scipy.optimize.linprog(c, A_eq=a, b_eq=b, method='highs')
t = time.time() - t0

if s.status == 0 or s.status == 1:
    print(s.fun, t)
else:
    print("None", t)