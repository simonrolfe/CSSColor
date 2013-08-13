CSSColor
========

Converts a CSS colour to a .NET Color object.

Please use this however you'd like - as a compiled class, code within your project, or anything else. 

I'd love to hear about it if you do.

It's pretty simple to use - just feed the static `FromCSSString` function a valid CSS colour string: This can be in `#rrggbb`, `#rgb`, `#rrggbaa`, `rgb(r, g, b)`, `rgba(r, g, b, a)`, `hsl(h, s, l)`, `hsla(h, s, l, a)`, or a named CSS colour, and it'll spit out a .NET `System.Drawing.Color` object matching the one that's input.

Any invalid CSS colour string will cause an error, although out-of-range values will be clamped per the CSS specs.
