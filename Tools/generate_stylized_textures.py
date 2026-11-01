"""Create seamless, full-colour hand-painted style albedo tiles for Turbo Boats."""

from pathlib import Path
import math

import numpy as np
from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1] / "Assets/Resources/TurboBoats/Textures"
SIZE = 512


def grid():
    u = np.arange(SIZE, dtype=np.float32)[None, :] / SIZE
    v = np.arange(SIZE, dtype=np.float32)[:, None] / SIZE
    return np.broadcast_to(u, (SIZE, SIZE)), np.broadcast_to(v, (SIZE, SIZE))


def periodic_noise(u, v, seed, octaves=5):
    rng = np.random.default_rng(seed)
    result = np.zeros((SIZE, SIZE), np.float32)
    weight = 1.0
    total = 0.0
    for octave in range(octaves):
        fx = 2 ** (octave + 1)
        fy = 2 ** (octave + 1)
        phase_x, phase_y = rng.uniform(0, math.tau, 2)
        phase_xy, phase_yx = rng.uniform(0, math.tau, 2)
        result += weight * (
            np.sin(math.tau * fx * u + phase_x)
            * np.sin(math.tau * fy * v + phase_y)
            + 0.48 * np.sin(math.tau * (fx + 1) * u + math.tau * fy * v + phase_xy)
            + 0.32 * np.sin(math.tau * fx * u - math.tau * (fy + 1) * v + phase_yx)
        )
        total += weight * 1.8
        weight *= 0.52
    return result / total


def colourize(name, rgb, variation, seed):
    u, v = grid()
    noise = periodic_noise(u, v, seed)
    shade = np.clip(1.0 + variation * noise, 0.72, 1.28)
    pixels = np.empty((SIZE, SIZE, 3), np.uint8)
    for channel, value in enumerate(rgb):
        pixels[:, :, channel] = np.clip(value * shade, 0, 255).astype(np.uint8)
    Image.fromarray(pixels, "RGB").save(ROOT / f"{name}_albedo.png")


def water():
    u, v = grid()
    noise = periodic_noise(u, v, 1701, 6)
    wave = np.sin(math.tau * (3.0 * v + 0.22 * np.sin(math.tau * 2.0 * u)))
    ripple = np.sin(math.tau * (11.0 * v + 0.35 * np.sin(math.tau * 3.0 * u)))
    light = np.clip(0.88 + 0.075 * wave + 0.035 * ripple + 0.07 * noise, 0.62, 1.18)
    pixels = np.stack((24 * light, 117 * light, 112 * light), axis=2).clip(0, 255).astype(np.uint8)
    image = Image.fromarray(pixels, "RGB")
    draw = ImageDraw.Draw(image)
    rng = np.random.default_rng(1702)
    for _ in range(66):
        x = int(rng.integers(0, SIZE))
        y = int(rng.integers(0, SIZE))
        length = int(rng.integers(5, 28))
        color = (118, 215, 192) if rng.random() < 0.75 else (239, 218, 153)
        for offset in (-SIZE, 0, SIZE):
            draw.line((x + offset, y, x + length + offset, y), fill=color, width=int(rng.integers(1, 3)))
    image.save(ROOT / "canal_water_albedo.png")


def timber(name, base, seed):
    u, v = grid()
    noise = periodic_noise(u, v, seed, 5)
    board_count = 5
    board = np.sin(math.tau * board_count * v)
    grain = np.sin(math.tau * (13 * u + 0.20 * np.sin(math.tau * 2 * v)))
    knot = np.exp(-((np.minimum(np.abs(u - 0.28), 1 - np.abs(u - 0.28)) / 0.09) ** 2
                    + (np.minimum(np.abs(v - 0.63), 1 - np.abs(v - 0.63)) / 0.045) ** 2))
    shade = np.clip(1 + 0.105 * noise + 0.055 * grain - 0.13 * (board > 0.985) - 0.15 * knot, 0.55, 1.28)
    pixels = np.empty((SIZE, SIZE, 3), np.uint8)
    for channel, value in enumerate(base):
        pixels[:, :, channel] = np.clip(value * shade, 0, 255).astype(np.uint8)
    image = Image.fromarray(pixels, "RGB")
    draw = ImageDraw.Draw(image)
    for row in range(board_count + 1):
        y = round(row * SIZE / board_count)
        draw.line((0, y, SIZE - 1, y), fill=(68, 35, 22), width=2)
        draw.line((0, (y + 2) % SIZE, SIZE - 1, (y + 2) % SIZE), fill=(203, 126, 66), width=1)
    image.save(ROOT / f"{name}_albedo.png")


def roof():
    image = Image.new("RGB", (SIZE, SIZE), (176, 69, 37))
    draw = ImageDraw.Draw(image)
    tile_w, tile_h = 64, 42
    for row, y in enumerate(range(-tile_h, SIZE + tile_h, tile_h)):
        shift = 0 if row % 2 == 0 else tile_w // 2
        for x in range(-tile_w, SIZE + tile_w, tile_w):
            x0 = x + shift
            col = row % 3
            color = ((210, 97, 48), (193, 77, 38), (226, 119, 57))[col]
            for offset in (-SIZE, 0, SIZE):
                xx = x0 + offset
                draw.rounded_rectangle((xx + 2, y + 2, xx + tile_w - 2, y + tile_h + 4), radius=9, fill=color)
                draw.arc((xx + 5, y + 3, xx + tile_w - 6, y + tile_h + 6), 180, 350, fill=(244, 153, 81), width=3)
                draw.line((xx + 8, y + tile_h - 4, xx + tile_w - 8, y + tile_h - 4), fill=(142, 51, 31), width=2)
    roof_noise = periodic_noise(*grid(), 3001, 4)
    base = np.asarray(image).astype(np.float32)
    base *= (1.0 + 0.035 * roof_noise[:, :, None])
    Image.fromarray(base.clip(0, 255).astype(np.uint8), "RGB").save(ROOT / "market_terracotta_albedo.png")


def canvas():
    u, v = grid()
    weave = 0.035 * np.sin(math.tau * 42 * u) + 0.035 * np.sin(math.tau * 42 * v)
    noise = periodic_noise(u, v, 4201, 4)
    shade = np.clip(1.0 + weave + 0.04 * noise, 0.88, 1.12)
    pixels = np.stack((246 * shade, 214 * shade, 160 * shade), axis=2).clip(0, 255).astype(np.uint8)
    Image.fromarray(pixels, "RGB").save(ROOT / "market_canvas_albedo.png")


def main():
    ROOT.mkdir(parents=True, exist_ok=True)
    water()
    timber("longtail_teak", (145, 73, 34), 1101)
    timber("market_timber", (125, 67, 37), 1201)
    roof()
    canvas()


if __name__ == "__main__":
    main()
