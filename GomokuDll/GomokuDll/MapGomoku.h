#pragma once

#include <iostream>
#include <cstdint>
#include <bitset>
#include <string.h>
#include <fstream>


#define MAP_H 19
#define BLACK 3
#define WHITE 1
#define EMPTY 0
# define ELSE 0
# define XOR(X, Y) ((uint64_t)X ^ (uint64_t)Y)
# define AND(X, Y) ((uint64_t)X & (uint64_t)Y)
# define BIT(X, Y) ((uint64_t)((uint64_t)X << Y * 2))
# define PUT_PIECE(MAP, X, Y, COLOR) (MAP[Y] = XOR(XOR(AND(BIT(3, X), MAP[Y]), BIT(COLOR, X)), MAP[Y]))
# define MAP_AT(MAP, X, Y) ((MAP[Y] >> (X * 2)) & 3)
# define IS_OUT(X, Y) (X < 0 || X >= MAP_H || Y < 0 || Y >= MAP_H)

class   Map_gomoku
{
public:
	Map_gomoku();
	~Map_gomoku();
	Map_gomoku(const Map_gomoku &copy);
	Map_gomoku	&operator=(const Map_gomoku &copy) { return *this; }

	std::ofstream fichierMap;
	int         getPiece(int, int) const;
	int        setPiece(int, int, int);
	void        printMap() ;
	bool        isEmpty(int, int) const;
	uint64_t    *getMap() const;
	void	setMap(uint64_t *);
private:
	uint64_t    *_map;
};

