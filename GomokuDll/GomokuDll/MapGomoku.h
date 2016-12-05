#pragma once

#include <iostream>
#include <cstdint>
#include <bitset>

#define MAP_H 20
#define BLACK 0
#define WHITE 1
#define EMPTY 3

class   Map_gomoku
{
public:
	Map_gomoku();
	~Map_gomoku();
	int         getPiece(int, int) const;
	void         setPiece(int, int, int);
	void        printMap() const;
	bool        isEmpty(int, int) const;
	uint64_t    *getMap() const;
private:
	uint64_t    *_map;
};

