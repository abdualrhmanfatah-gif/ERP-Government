const t = require('./src/design-system/tokens.tailwind.json');
const colors = t.colors;
const allVars = new Set();
function extractVars(obj, prefix) {
  for (const [k, v] of Object.entries(obj)) {
    if (typeof v === 'object' && v !== null) extractVars(v, prefix ? `${prefix}.${k}` : k);
    else if (typeof v === 'string' && v.includes('var(')) {
      const match = v.match(/var\(([^)]+)\)/);
      if (match) allVars.add(match[1]);
    }
  }
}
extractVars(colors, '');
console.log([...allVars].sort().join('\n'));
