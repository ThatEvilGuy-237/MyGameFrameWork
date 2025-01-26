using MyGameFrameWork.Framework;

namespace MyGameFrameWork
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            using (Window game = new Window(800, 800, "test"))
            {
                game.Run(); // Run the game at 60 frames per second
            };
        }
    }
}
