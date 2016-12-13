#pragma once

#include <random>
#include <cstdlib>
#include <iostream>
#include <ctime>
#include <list>
#include <iostream>
#include <string.h>
#include <stdlib.h>
#include <vector>
//#include <unistd.h>
#include <iostream>     // std::cout
#include <algorithm>    // std::find
#include <vector>
#include "MapGomoku.h"
#include "GomokuApi.h"


# define DIRX {0, 1, 1, 1, 0, -1, -1, -1}
# define DIRY {1, 1, 0, -1, -1, -1, 0, 1}

# define EAT_PAT1 (WHITE | (BLACK << 2) | (BLACK << 4) | (WHITE << 6))
# define EAT_PAT2 (BLACK | (WHITE << 2) | (WHITE << 4) | (BLACK << 6))

# define VUL_PAT1 (BLACK | (WHITE << 2) | (WHITE << 4) | (EMPTY << 6))
# define VUL_PAT2 (EMPTY | (WHITE << 2) | (WHITE << 4) | (BLACK << 6))
# define VUL_PAT3 (WHITE | (BLACK << 2) | (BLACK << 4) | (EMPTY << 6))
# define VUL_PAT4 (EMPTY | (BLACK << 2) | (BLACK << 4) | (WHITE << 6))

# define HEUR_WIN (10)
# define HEUR_LOSE (-10)
# define HEUR_EAT (2)
# define HEUR_EATEN (-2)
# define HEUR_FOUR (4)
# define HEUR_THREE (2)
# define HEUR_TWO (1)

typedef struct s_coor
{
	int x;
	int y;
} coor;

typedef struct s_possibility
{
	int     score;
	s_coor  pos;
} possibility;

class IAGomoku
{
public:
	IAGomoku();
	~IAGomoku() {}
	std::ofstream fichierMapa;

	std::list<coor>	choose_places(uint64_t *map, int x, int y);
	coor			random_place(uint64_t *map, int x, int y);
	void             *runIA(int color, std::list<coor>);
	void			setIA(GomokuApi *game, bool rule_brk);
	std::vector<possibility>            _solutions;
	int				getTime() { return _timeIa; };
	void				setTime(int time) { _timeIa = time; };
	int				getPos() { return _posToSend; };
	void				setPos(int pos) { _posToSend = pos; };
private:
	void			sort_score(std::vector<possibility> &);
	void			monte_carlo(coor, coor);
	void			points_branch(int, coor, Map_gomoku &, coor);
	int			color_win_branch(int, coor, Map_gomoku &, coor);
	int			check_win(const Map_gomoku &, coor) const;
	int			eat(Map_gomoku &, coor) const;
	int			eat_dir(Map_gomoku &, coor, int, int) const;
	int			check_5_align_board(const Map_gomoku &) const;
	int			check_var_align(const Map_gomoku &map, coor place, int x_inc, int y_inc) const;
	int			check_5_align(const Map_gomoku &, coor, int, int) const;
	int			check_breakable(const Map_gomoku&, coor, int, int) const;
	bool			check_if_vulnerable(const Map_gomoku &map, coor place, int x_inc, int y_inc) const;
	int				_timeIa;

	// std::random_device    _rd;
	// std::mt19937		      *_gen;
	std::default_random_engine          _generator;
	//std::uniform_int_distribution<int> _distribution(0, 3);
	//std::uniform_int_distribution<int>(0,2);
	//std::vector<possibility>            _solutions;
	GomokuApi			*_api;
	Map_gomoku         _game;
	int					_posToSend;
	bool			         _rule_brk;
	int			           _color;
	int			           _points;
	int			           _depth;
};

extern "C" {
	MYGOMOKU_API IAGomoku* CreateIAGomoku();
	MYGOMOKU_API void DeleteIAGomoku(IAGomoku *ia);
	MYGOMOKU_API void SetIa(IAGomoku *ia, GomokuApi *api);
	MYGOMOKU_API void RunIa(IAGomoku *ia, GomokuApi *api, int color, int pos);
	MYGOMOKU_API int GetPos(IAGomoku *api);
	MYGOMOKU_API int GetTime(IAGomoku *api);
}
