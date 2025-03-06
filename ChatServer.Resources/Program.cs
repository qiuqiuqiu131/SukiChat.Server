namespace ChatServer.Resources
{
    class Program
    {
        static void Main(string[] args)
        {
            App app = new App();
            app.Start().Wait();
            Console.ReadLine();
            app.Close().Wait();
        }
    }
}