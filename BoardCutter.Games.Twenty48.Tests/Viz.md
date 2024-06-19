```
- - - - 
- - - - 
- - - - 
- - - -
```

```
2 2 2 4
0 0 4 4
0 0 2 8
2 0 4 8
```

=>

https://en.wikipedia.org/wiki/2048_(video_game)#Gameplay


 2048 is played on a plain 4×4 grid, with numbered tiles that slide when a player moves them using the four arrow keys.[4] The game begins with two tiles already in the grid, having a value of either 2 or 4, and another such tile appears in a random empty space after each turn.[5] Tiles slide as far as possible in the chosen direction until they are stopped by either another tile or the edge of the grid. If two tiles of the same number collide while moving, they will merge into a tile with the total value of the two tiles that collided.[6][7] The resulting tile cannot merge with another tile again in the same move. Higher-scoring tiles emit a soft glow;[5] the largest possible tile is 131,072.[8]
 
 If a move causes three consecutive tiles of the same value to slide together, only the two tiles farthest along the direction of motion will combine. If all four spaces in a row or column are filled with tiles of the same value, a move parallel to that row/column will combine the first two and last two.[9] A scoreboard on the upper-right keeps track of the user's score. The user's score starts at zero, and is increased whenever two tiles combine, by the value of the new tile.[5]


- `2 2 2 2` => `- - 4 4`
- `2 2 2 -` => `- - 2 4`
- `- 2 - 2` => `- - - 4`
- `- 4 2 2` => `- - 4 4`