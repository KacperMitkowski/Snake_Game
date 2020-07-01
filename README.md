# Snake Game in C# and WPF

This is a simple tile-based platform game in C# and WPF

## About the project

This is a game made in my free time. The project is easy to extend by adding new functionalities such as: new types of enemies, adding walls, adding different types of difficulty, adding new items etc. 

**The goal was to:**
- practice OOP concept in C#
- create a game loop with the option exit, restart, quit game
- use serialization / deserialization to store winners in high score list in XML file 
- use WPF as a GUI

## Used technologies:
- C#
- WPF

## Rules:

1. Player moves the character by pressing arrows (ESC is exit from game)
2. There is only one enemy which bounds from the wall (amount of the enemies is determined in the code)
3. Snake cannot touch the enemy or its own tail. If player touches it - the game stops and it must be started from the scratch
4. When snake eats the apple it becomes longer and it moves faster
5. Player can see high score list (read from XML file) and can exit game by pressing ESC

## Screenshots:
![alt text](https://github.com/KacperMitkowski/Snake_Game/blob/master/Snake_Game/screenshots/screenshots_1.gif)
