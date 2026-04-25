const { spawn } = require('child_process');
const path = require('path');

const root = process.cwd();
const apiProject = path.join(root, 'AiTodoApp.Api', 'AiTodoApp.Api.csproj');
const webPath = path.join(root, 'meetingsplus-web');

console.log('Starting MeetingsPlus services...');
console.log('Swagger URL: https://localhost:9011/swagger');
console.log('UI URL:      http://localhost:3000');
console.log('');

const apiProcess = spawn('dotnet', ['run', '--project', apiProject, '--launch-profile', 'https'], {
  stdio: 'inherit',
  shell: true,
});

const webProcess = spawn('npm', ['run', 'dev', '--', '--host', '0.0.0.0', '--port', '3000'], {
  cwd: webPath,
  stdio: 'inherit',
  shell: true,
});

const shutdown = () => {
  if (!apiProcess.killed) apiProcess.kill();
  if (!webProcess.killed) webProcess.kill();
};

process.on('SIGINT', () => {
  shutdown();
  process.exit(0);
});

process.on('SIGTERM', () => {
  shutdown();
  process.exit(0);
});

apiProcess.on('exit', (code) => {
  if (!webProcess.killed) webProcess.kill();
  process.exit(code ?? 0);
});

webProcess.on('exit', (code) => {
  if (!apiProcess.killed) apiProcess.kill();
  process.exit(code ?? 0);
});
