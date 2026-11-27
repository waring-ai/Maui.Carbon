# Third-party notices

Maui.Carbon is an unofficial .NET MAUI implementation of the Carbon Design System. It is not affiliated with,
sponsored by or endorsed by IBM. "Carbon" and "IBM Plex" are names used by IBM.

## Carbon Design System (tokens, component specifications, icons)

Design tokens (`design/carbon/tokens.json`), component specifications and the icon artwork in
`design/carbon/icons/` (and the path data generated from it in `src/Maui.Carbon/Generated/`) derive from the
Carbon Design System, https://github.com/carbon-design-system/carbon, and `@carbon/icons`.

Copyright IBM Corp. Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0).
See `design/carbon/icons/LICENSE-carbon-icons`.

## Carbon for IBM Products (component specifications)

The controls in `src/Maui.Carbon/Components/Products/` are a C# implementation of components from Carbon for IBM
Products, https://github.com/carbon-design-system/ibm-products (`@carbon/ibm-products`), following its component
behavior, measurements and usage guidance. Copyright IBM Corp. Licensed under the Apache License, Version 2.0
(http://www.apache.org/licenses/LICENSE-2.0).

## IBM Plex (fonts)

The IBM Plex Sans, IBM Plex Sans Condensed, IBM Plex Mono and IBM Plex Serif font files embedded in the package
(`src/Maui.Carbon/Resources/Fonts/`) are Copyright © 2017 IBM Corp. with Reserved Font Name "Plex", licensed under
the SIL Open Font License, Version 1.1. See `design/carbon/fonts/OFL.txt` or https://openfontlicense.org.

## Carbon Charts and the libraries bundled with it (design system charts)

The chart components in `design/carbon/components/bundle.js` (built by `tools/carbon-charts`) are a hand-written
port of Carbon Charts, https://github.com/carbon-design-system/carbon-charts (`@carbon/charts` 1.27), Copyright IBM
Corp., licensed under the Apache License, Version 2.0. The build bundles parts of:

- d3 7.9.0 modules (d3-array, d3-scale, d3-shape, d3-hierarchy, d3-geo and their dependencies), Copyright Mike
  Bostock, ISC License.
- d3-sankey 0.12.3, Copyright Mike Bostock, BSD 3-Clause License.
- d3-cloud 1.2.7, Copyright Jason Davies, BSD 3-Clause License.
- topojson-client 3.1.0, Copyright Mike Bostock, ISC License.

The MAUI chart controls (`src/Maui.Carbon/Components/Charts/`) are a C# port of the same design system components:
the scale, tick, curve, hierarchy, layout and projection algorithms are rewritten from the d3 modules above (ISC and
BSD 3-Clause licenses), and the chart palettes come from `@carbon/colors` (Apache License, Version 2.0) through
`tools/carbon-charts/build-maui.mjs`.

`design/carbon/assets/Maps/countries-110m.json` (and the gallery's copy in `samples/Maui.Carbon.Gallery/Resources/Raw/Maps/`) is world-atlas 2.0.2 (Copyright Mike Bostock, ISC License), derived
from Natural Earth (public domain).
