#version 330 core

// Generic CRT fragment shader.
// Expected uniforms: source, resolution, time.
uniform sampler2D source;
uniform vec2 resolution;
uniform float time;
uniform float curvature;          // 0.0 = off, 0.08 = subtle curve
uniform float scanline_strength;  // 0.0 - 1.0
uniform float glow_strength;      // 0.0 - 1.0

in vec2 uv;
out vec4 fragColor;

float hash(vec2 p) {
  p = fract(p * vec2(123.34, 456.21));
  p += dot(p, p + 45.32);
  return fract(p.x * p.y);
}

vec2 curve(vec2 p, float amount) {
  vec2 c = p * 2.0 - 1.0;
  c *= 1.0 + amount * dot(c, c);
  return c * 0.5 + 0.5;
}

void main() {
  float c = clamp(curvature, 0.0, 0.2);
  vec2 mapped = curve(uv, c);

  if (mapped.x < 0.0 || mapped.x > 1.0 || mapped.y < 0.0 || mapped.y > 1.0) {
    fragColor = vec4(0.03, 0.03, 0.02, 1.0);
    return;
  }

  vec3 base = texture(source, mapped).rgb;

  float lines = sin(mapped.y * resolution.y * 3.14159265);
  float scan = mix(1.0, 0.84 + 0.16 * lines, clamp(scanline_strength, 0.0, 1.0));

  float noise = hash(vec2(floor(time * 24.0), floor(mapped.y * resolution.y * 0.25)));
  float variance = 0.97 + 0.05 * noise;

  vec2 texel = 1.0 / resolution;
  vec3 bloom = (
    texture(source, mapped + vec2(texel.x, 0.0)).rgb +
    texture(source, mapped - vec2(texel.x, 0.0)).rgb +
    texture(source, mapped + vec2(0.0, texel.y)).rgb +
    texture(source, mapped - vec2(0.0, texel.y)).rgb
  ) * 0.25;

  vec3 crt = base * scan * variance;
  crt += bloom * (0.16 * clamp(glow_strength, 0.0, 1.0));

  // Slight phosphor tint bias toward warm amber.
  crt *= vec3(1.04, 1.0, 0.9);

  fragColor = vec4(crt, 1.0);
}
