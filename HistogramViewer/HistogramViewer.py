
import json
import matplotlib.pyplot as plt
import numpy as np

# Load the serialized JSON file
with open("image_assets.json", "r") as f:
    data = json.load(f)

if not data:
    print("No ImageAssets found in JSON.")
    exit()

# Global index for current image
current_index = 0

def plot_histogram(index):
    """ Plots the histogram of the image at the given index. """
    global current_index
    current_index = index % len(data)  # Ensure wrap-around

    # Get histogram data
    asset = data[current_index]
    histogram = np.array(asset["ColorHistogram"])

    # Clear plot and draw new histogram
    plt.clf()
    plt.bar(range(len(histogram)), histogram, color="blue")
    plt.title(f"Histogram of {asset['Id']} ({current_index + 1}/{len(data)})")
    plt.xlabel("Bins (Hue + Saturation + Value)")
    plt.ylabel("Normalized Frequency")
    plt.draw()

def on_key(event):
    """ Handles keyboard input to navigate images. """
    global current_index
    if event.key == "right":  # Next image
        plot_histogram(current_index + 1)
    elif event.key == "left":  # Previous image
        plot_histogram(current_index - 1)

# Set up interactive figure
plt.figure(figsize=(10, 5))
plot_histogram(0)  # Show first histogram initially
plt.gcf().canvas.mpl_connect("key_press_event", on_key)  # Bind key events
plt.show()
