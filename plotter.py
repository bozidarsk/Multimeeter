import matplotlib.pyplot as plt
import numpy as np
import sys
from datetime import datetime

start = datetime.now()
xpoints = []
ypoints = []

def plot_value(y):
	x = (datetime.now() - start).total_seconds()

	print(f"{x},{y}")

	xpoints.append(x)
	ypoints.append(y)
	plt.plot(xpoints, ypoints)
	plt.pause(0.05)

for line in sys.stdin:
	if line == "reset\r\n":	
		start = datetime.now()
		xpoints = []
		ypoints = []
		sys.stdin.flush()
		plt.clf()
	else:
		plot_value(float(line))
