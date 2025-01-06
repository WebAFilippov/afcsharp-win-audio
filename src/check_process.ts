import { getProcessesByName } from 'node-processlist';

setInterval(() => {
  (async () => {
    const processes = await getProcessesByName('af-win-audio.exe');
    console.log("=====================================");
    console.log(processes);
    console.log("=====================================");

  })();}, process.argv[2] ? parseInt(process.argv[2]) : 2000)