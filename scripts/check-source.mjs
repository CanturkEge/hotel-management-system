// Lightweight static checks only. These do not compile C#, Razor, or PowerShell.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
function walk(dir) { return fs.readdirSync(dir, {withFileTypes:true}).flatMap(e => e.isDirectory() ? walk(path.join(dir,e.name)) : [path.join(dir,e.name)]); }
const files = walk(root);
const fail = message => { throw new Error(message); };
for (const name of files.filter(f=>f.endsWith('.json'))) JSON.parse(fs.readFileSync(name,'utf8'));
for (const name of files.filter(f=>f.endsWith('.csproj'))) {
  const source = fs.readFileSync(name,'utf8');
  for (const match of source.matchAll(/ProjectReference Include="([^"]+)"/g)) {
    if (!fs.existsSync(path.resolve(path.dirname(name),match[1]))) fail('Missing project reference: '+match[1]);
  }
}
const sln = fs.readFileSync(path.join(root,'HotelManagement.sln'),'utf8');
for (const match of sln.matchAll(/"([^"]+\.csproj)"/g)) {
  if (!fs.existsSync(path.join(root,match[1].replaceAll('\\','/')))) fail('Missing solution project: '+match[1]);
}
const web=path.join(root,'src/HotelManagement.Web');
const actions=new Map();
for (const f of files.filter(f=>f.endsWith('Controller.cs'))) {
  const source=fs.readFileSync(f,'utf8');
  const name=path.basename(f).replace('Controller.cs','');
  actions.set(name,new Set([...source.matchAll(/(?:Task<IActionResult>|IActionResult)\s+(\w+)\s*\(/g)].map(x=>x[1])));
}
for (const f of files.filter(f=>f.endsWith('.cshtml'))) {
  const source=fs.readFileSync(f,'utf8');
  for (const m of source.matchAll(/(?:src|href)="~\/([^"@]+)"/g)) {
    if (!fs.existsSync(path.join(web,'wwwroot',m[1]))) fail('Missing local asset: '+m[1]);
  }
  const ownController=path.basename(path.dirname(f));
  for (const tag of source.matchAll(/<(?:form|a)\b[^>]*>/g)) {
    const action=tag[0].match(/asp-action="(\w+)"/)?.[1];
    if (!action)continue;
    const controller=tag[0].match(/asp-controller="(\w+)"/)?.[1] ?? ownController;
    if(!actions.get(controller)?.has(action))fail(`Unknown action in ${f}: ${controller}/${action}`);
  }
}
const program=fs.readFileSync(path.join(web,'Program.cs'),'utf8');
if(!program.includes('AutoValidateAntiforgeryTokenAttribute')) fail('Missing global antiforgery filter');
for(const controller of ['Bookings','Staff','Management','Jobs','Users']) {
  const source=fs.readFileSync(path.join(web,'Controllers',controller+'Controller.cs'),'utf8');
  if(!source.includes('[Authorize(Roles='))fail('Missing role gate: '+controller);
}
const testSource=fs.readFileSync(path.join(root,'tests/HotelManagement.Tests/Program.cs'),'utf8');
console.log('Static file/JSON/project/route checks passed.');
console.log('Files:',files.length,'C# source files:',files.filter(f=>f.endsWith('.cs')).length,'Razor views:',files.filter(f=>f.endsWith('.cshtml')).length);
console.log('Business-rule test definitions:',[...testSource.matchAll(/await Test\(/g)].length,'(not executed by this script)');
