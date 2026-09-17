#!/usr/bin/env node
// Report-only check: finds var(--color-*) / var(--status-*) references that the
// generated token stylesheet does not define. Not a build gate: exits 0 unless
// --strict is passed. Run from src/Web/ClientApp via `npm run design:usage`.

import { readFileSync, readdirSync, statSync } from 'node:fs';
import { join, relative, resolve } from 'node:path';

const cwd = process.cwd();
const cssFile = join(cwd, 'src', 'design-system', 'tokens.css.scss');
const srcDir = join(cwd, 'src');
const strict = process.argv.includes('--strict');

const css = readFileSync(cssFile, 'utf8');
const defined = new Set();
for (const match of css.matchAll(/(--[a-zA-Z0-9-]+)\s*:/g)) {
  defined.add(match[1]);
}

const checkedPrefixes = ['--color-', '--status-'];
const files = [];

function walk(dir) {
  for (const entry of readdirSync(dir)) {
    if (entry === 'node_modules' || entry.startsWith('.')) continue;
    const full = join(dir, entry);
    if (statSync(full).isDirectory()) {
      walk(full);
    } else if (/\.(ts|tsx)$/.test(entry)) {
      files.push(full);
    }
  }
}

walk(srcDir);

const offenders = [];
for (const file of files) {
  const text = readFileSync(file, 'utf8');
  const lines = text.split(/\r?\n/);
  lines.forEach((line, index) => {
    for (const match of line.matchAll(/var\(\s*(--[a-zA-Z0-9-]+)/g)) {
      const name = match[1];
      if (!checkedPrefixes.some((prefix) => name.startsWith(prefix))) continue;
      if (!defined.has(name)) {
        offenders.push({ file: relative(cwd, file).replace(/\\/g, '/'), line: index + 1, name });
      }
    }
  });
}

const byName = new Map();
for (const offender of offenders) {
  byName.set(offender.name, (byName.get(offender.name) ?? 0) + 1);
}

console.log(`token-usage scan: ${files.length} files, ${defined.size} defined variables, ${offenders.length} unknown reference(s)`);
for (const [name, count] of [...byName.entries()].sort((a, b) => b[1] - a[1])) {
  console.log(`  ${name} — ${count} reference(s)`);
}
const uiOffenders = offenders.filter((o) => o.file.includes('components/ui/'));
console.log(`  in components/ui/: ${uiOffenders.length}`);
if (uiOffenders.length > 0 && offenders.length <= 60) {
  for (const offender of uiOffenders) {
    console.log(`    ${offender.file}:${offender.line} ${offender.name}`);
  }
}

if (strict && uiOffenders.length > 0) {
  process.exitCode = 1;
}
