using QA_VS;

if (args.Length != 4)
{
    Console.WriteLine("You should use 4 arguments");
    return;
}

string sourcePath = args[0];
string replicaPath = args[1];
int interval = int.Parse(args[2]);
string logPath = args[3];

Logger logger = new Logger(logPath);
Synchronizer synchronizer = new Synchronizer(logger);


while (true)
{
    synchronizer.Synchronize(sourcePath, replicaPath);
    logger.Log("Synchronized");
    Thread.Sleep(interval * 1000);
}