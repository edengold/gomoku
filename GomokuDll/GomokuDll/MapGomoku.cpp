#include "MapGomoku.h"

Map_gomoku::Map_gomoku()
{
	_map = new uint64_t[MAP_H];
}

Map_gomoku::~Map_gomoku()
{
	delete _map;
}

int     Map_gomoku::getPiece(int x, int y) const
{
	return ((_map[y] >> (x * 2)) & 3);
}

void     Map_gomoku::setPiece(int x, int y, int type)
{
	_map[y] = _map[y] | (type << (x * 2));
}

void    Map_gomoku::printMap() const
{
	for (int y = 0; y < MAP_H; y++)
	{
		for (int x = 0; x < MAP_H; x++)
		{
			if (getPiece(x, y) == WHITE)
				std::cout << "■";
			else if (getPiece(x, y) == BLACK)
				std::cout << "□";
			else
				std::cout << "┼";
			if (x < (MAP_H - 1))
				std::cout << "─";
		}
		std::cout << std::endl;
	}
}

bool    Map_gomoku::isEmpty(int x, int y) const
{
	if (getPiece(x, y) == EMPTY)
		return true;
	return false;
}

uint64_t    *Map_gomoku::getMap() const
{
	return (_map);
}

