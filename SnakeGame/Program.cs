using System;
using static System.Formats.Asn1.AsnWriter;
using System.IO;
using System.Security.Cryptography;

namespace SnakeGame
{
    public class SnakeGame
    {
        private const int ConsoleWidth = 80;
        private const int ConsoleHeight = 25;
        private const char SnakeSymbol = 'O';
        private const char FoodSymbol = '@';
        private const char CharacterSymbol = 'M';

        private static readonly Random random = new Random();
        private static bool gameover;
        private static int score;
        private static int lives;

        private static int characterX;
        private static int characterY;
        private static int snakeHeadX;
        private static int snakeHeadY;
        private static int foodX;
        private static int foodY;
        private static int directionX;
        private static int directionY;
        private static DateTime lastHealTime;

        private static List<int> snakeX;
        private static List<int> snakeY;

        public static void Main()
        {
            Console.Title = "Змейка и персонаж";
            Console.CursorVisible = false;
            Console.SetWindowSize(ConsoleWidth, ConsoleHeight);
            Console.SetBufferSize(ConsoleWidth, ConsoleHeight);
            Console.ForegroundColor = ConsoleColor.Green;

            InitializeGame();

            // Основной игровой цикл
            while (!gameover)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    HandleInput(key);
                }

                MoveCharacter();
                MoveSnake();

                if (!gameover)
                {
                    DrawGame();
                }

                Thread.Sleep(100);
                if (lives > 0)
                {
                    gameover = false;

                }
                else
                {
                    gameover = true;
                }
            }

            Console.Clear();
            Console.SetCursorPosition(ConsoleWidth / 2 - 4, ConsoleHeight / 2);
            Console.WriteLine("Игра окончена!");
            Console.SetCursorPosition(ConsoleWidth / 2 - 4, ConsoleHeight / 2 + 1);
            Console.WriteLine("Очки: " + score);
            Console.SetCursorPosition(ConsoleWidth / 2 - 4, ConsoleHeight / 2 + 2);
            Console.WriteLine("Жизни: " + lives);
            Console.SetCursorPosition(ConsoleWidth / 2 - 11, ConsoleHeight / 2 + 3);
            Console.WriteLine("Нажмите любую кнопку для выхода...");
            Console.ReadKey();
            Main();

        }



        private static void InitializeGame()
        {
            gameover = false;
            score = 0;
            lives = 3;

            characterX = ConsoleWidth / 2;
            characterY = ConsoleHeight / 2;

            snakeHeadX = random.Next(1, ConsoleWidth - 1);
            snakeHeadY = random.Next(1, ConsoleHeight - 1);

            foodX = random.Next(1, ConsoleWidth - 1);
            foodY = random.Next(1, ConsoleHeight - 1);

            directionX = 1;
            directionY = 0;

            snakeX = new List<int>() { snakeHeadX };
            snakeY = new List<int>() { snakeHeadY };

            lastHealTime = DateTime.Now;
        }

        private static void DrawGame()
        {
            Console.Clear();

            // Отрисовка змеи
            Console.SetCursorPosition(snakeHeadX, snakeHeadY);
            Console.Write(SnakeSymbol);

            for (int i = 1; i < snakeX.Count; i++)
            {
                Console.SetCursorPosition(snakeX[i], snakeY[i]);
                Console.Write(SnakeSymbol);
            }

            // Отрисовка персонажа
            Console.SetCursorPosition(characterX, characterY);
            Console.Write(CharacterSymbol);

            // Отрисовка пищи
            Console.SetCursorPosition(foodX, foodY);
            Console.Write(FoodSymbol);

            // Отрисовка счета и жизней
            Console.SetCursorPosition(0, ConsoleHeight - 2);
            Console.Write("Очки: " + score);

            Console.SetCursorPosition(0, ConsoleHeight - 1);
            Console.Write("Жизни: " + lives);
        }


        private static void MoveCharacter()
        {
            Console.SetCursorPosition(characterX, characterY);
            Console.Write(" ");

            if (directionX != 0 || directionY != 0)
            {
                characterX += directionX;
                characterY += directionY;
            }

            if (characterX <= 0)
                characterX = ConsoleWidth - 1;
            if (characterX >= ConsoleWidth)
                characterX = 1;
            if (characterY <= 0)
                characterY = ConsoleHeight - 1;
            if (characterY >= ConsoleHeight)
                characterY = 1;

            // Восстановление здоровья
            if ((DateTime.Now - lastHealTime).TotalSeconds >= 10)
            {
                lastHealTime = DateTime.Now;
                if (lives < 3)
                    lives++;
            }
        }

        private static void MoveSnake()
        {
            int oldHeadX = snakeHeadX;
            int oldHeadY = snakeHeadY;

            // Движение змеи к персонажу
            if (snakeHeadX < characterX)
                snakeHeadX++;
            else if (snakeHeadX > characterX)
                snakeHeadX--;
            else if (snakeHeadY < characterY)
                snakeHeadY++;
            else if (snakeHeadY > characterY)
                snakeHeadY--;

            // Проверка соударения змеи с персонажем
            if (snakeHeadX == characterX && snakeHeadY == characterY)
            {
                gameover = true;
                lives--;
                if (lives <= 0)
                    return;
            }

            // Проверка соударения змеи со стенами
            if (snakeHeadX == 0 || snakeHeadX == ConsoleWidth - 1 ||
                snakeHeadY == 0 || snakeHeadY == ConsoleHeight - 1)
            {
                gameover = true;
                lives--;
                if (lives <= 0)
                    return;
            }

            // Проверка соударения змеи с самой собой
            for (int i = 1; i < snakeX.Count; i++)
            {
                if (snakeX[i] == snakeHeadX && snakeY[i] == snakeHeadY)
                {
                    gameover = true;
                    lives--;
                    if (lives <= 0)
                        return;
                }
            }

            // Проверка на поедание пищи
            if (snakeHeadX == foodX && snakeHeadY == foodY)
            {
                score++;
                if (score % 10 == 0)
                    lives++;
                GenerateFood();
                snakeX.Insert(0, oldHeadX);
                snakeY.Insert(0, oldHeadY);
            }

            // Обновление положения змеи
            for (int i = snakeX.Count - 1; i > 0; i--)
            {
                snakeX[i] = snakeX[i - 1];
                snakeY[i] = snakeY[i - 1];
            }

            snakeX[0] = snakeHeadX;
            snakeY[0] = snakeHeadY;
        }



        private static void GenerateFood()
        {
            bool foodOnSnake = true;

            while (foodOnSnake)
            {
                foodX = random.Next(1, ConsoleWidth - 1);
                foodY = random.Next(1, ConsoleHeight - 1);

                foodOnSnake = snakeX.Contains(foodX) && snakeY.Contains(foodY) ||
                    foodX == characterX && foodY == characterY;
            }
        }







        private static void HandleInput(ConsoleKeyInfo key)
        {
            var previousDirectionX = directionX;
            var previousDirectionY = directionY;

            switch (key.Key)
            {
                case ConsoleKey.W:
                    if (directionY != 1) // Запретить поворот на 180 градусов
                    {
                        directionX = 0;
                        directionY = -1;
                    }
                    break;

                case ConsoleKey.S:
                    if (directionY != -1) // Запретить поворот на 180 градусов
                    {
                        directionX = 0;
                        directionY = 1;
                    }
                    break;

                case ConsoleKey.A:
                    if (directionX != 1) // Запретить поворот на 180 градусов
                    {
                        directionX = -1;
                        directionY = 0;
                    }
                    break;

                case ConsoleKey.D:
                    if (directionX != -1) // Запретить поворот на 180 градусов
                    {
                        directionX = 1;
                        directionY = 0;
                    }
                    break;

                default:
                    break;
            }

            // Проверка перемещения персонажа
            if (directionX != previousDirectionX || directionY != previousDirectionY)
            {
                // Обработка перемещения персонажа
                int newCharacterX = characterX + directionX;
                int newCharacterY = characterY + directionY;

                // Проверка, чтобы персонаж не выходил за границы игрового поля
                if (newCharacterX >= 1 && newCharacterX <= ConsoleWidth - 2 && newCharacterY >= 1 && newCharacterY <= ConsoleHeight - 2)
                {
                    characterX = newCharacterX;
                    characterY = newCharacterY;
                }
            }
        }
    }
}