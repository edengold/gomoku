#include "IaGomoku.h"
#pragma region ForDll
MYGOMOKU_API IAGomoku* CreateIAGomoku()
{
	return new IAGomoku();
}

MYGOMOKU_API void DeleteIAGomoku(IAGomoku *ia)
{
	delete ia;
}

MYGOMOKU_API void SetIa(IAGomoku *ia, GomokuApi *api)
{
	ia->setIA(api, false);
}

MYGOMOKU_API void RunIa(IAGomoku *ia, GomokuApi *api, int color, int pos)
{
	int x = static_cast<int>(pos % MAP_H);
	int y = static_cast<int>(pos / MAP_H);
	std::ofstream fichierMap("IAPUATIN.txt", std::ios::out | std::ios::trunc);
	fichierMap << "LOG START\n";

	fichierMap << "x = " << x << " y = " << y << std::endl;

	ia->runIA(BLACK, ia->choose_places(api->get_board().getMap(), x, y));
}
MYGOMOKU_API int GetPos(IAGomoku *ia)
{
	if (ia->getPos() != -1)
	{
		int tmp = ia->getPos();
		ia->setPos(-1);
		return tmp;
	}
	return -1;
}

MYGOMOKU_API int GetTime(IAGomoku *ia)
{
	if (ia->getTime() != -1)
	{
		int tmp = ia->getTime();
		ia->setTime(-1);
		return tmp;
	}
	return -1;
}

#pragma endregion

std::list<coor>	IAGomoku::choose_places(uint64_t *map, int x, int y)
{
	std::list<coor>	places;
	coor			tmp;

	for (int x_inc = -2; x_inc <= 2; x_inc++)
	{
		for (int y_inc = -2; y_inc <= 2; y_inc++)
		{
			tmp.x = x + x_inc;
			tmp.y = y + y_inc;
			if (tmp.x >= 0 && tmp.y >= 0 && tmp.x < MAP_H && tmp.y < MAP_H)
			{
				if (MAP_AT(map, tmp.x, tmp.y) == EMPTY)
				{
					places.push_back(tmp);
				}
			}
		}
	}
	if (places.size() == 0)
	{
		for (tmp.x = 0; tmp.x < MAP_H; tmp.x++)
		{
			for (tmp.y = 0; tmp.y < MAP_H; tmp.y++)
			{
				if (MAP_AT(map, tmp.x, tmp.y) == EMPTY)
				{
					places.push_back(tmp);
				}
			}
		}

	}
	return (places);
}

IAGomoku::IAGomoku()
{
	_timeIa = -1;
	_posToSend = -1;
	_rule_brk = true;
	fichierMapa.open("testIAAZAZ.txt", std::ios::out | std::ios::trunc);
}

/*
void IAGomoku::setIA(const Arbitre &game, bool rule_brk)
{
uint64_t *new_map = new uint64_t[MAP_H];
Map_gomoku map_class = game.get_board();
uint64_t *tmp = map_class.getMap();

_rule_brk = rule_brk;
for (int i = 0; i < MAP_H; i++)
new_map[i] = tmp[i];
_game.setMap(new_map);
//_game.get_board().printMap();
}
*/

void IAGomoku::setIA(GomokuApi *game, bool rule_brk)
{
	_rule_brk = rule_brk;
	_api = game;
	_game = game->get_board();
	_generator = std::default_random_engine();
	//_rd = new std::random_device();
	//_gen = new std::mt19937(_rd);
	// std::random_device    _rd;
	// std::mt19937		gen(_rd());

}

void *IAGomoku::runIA(int color, std::list<coor> places)
{
	std::list<int>	scores;
	possibility     elem;
	coor		cpt_eat;
	float		res;
	std::clock_t    start;

	_color = color;
	_solutions.clear();
	start = std::clock();

	for (std::list<coor>::iterator place = places.begin(); place != places.end(); place++)
	{
		res = 0;
		for (int i = 0; i < 50; i++)
		{
			cpt_eat.x = 0;
			cpt_eat.y = 0;
			_points = 0;
			_depth = 10;
			monte_carlo(*place, cpt_eat);
			res += _points;
		}
		elem.score = res;
		elem.pos.x = (*place).x;
		elem.pos.y = (*place).y;
		_solutions.push_back(elem);
		//std::cout << res << " x:" << (*place).x << " y:" << (*place).y << std::endl;
		scores.push_back(res);
	}
	sort_score(_solutions);
	int finalpos = -1;
	for (int i = 0; i < _solutions.size(); i++)
	{
		if (!_api->is_double_3_align(_solutions[i].pos.x, _solutions[i].pos.y, BLACK))
		{
			fichierMapa << "x = " << _solutions[i].pos.x << " y = " << _solutions[i].pos.y << std::endl;
			finalpos = (_solutions[i].pos.y * 19) + _solutions[i].pos.x;
			break;;
		}
	}
	_timeIa = (std::clock() - start) / (double)(CLOCKS_PER_SEC / 1000);
	_api->CanIPutHere(finalpos);
	_posToSend = finalpos;

}

void	IAGomoku::monte_carlo(coor place, coor cpt_eat)
{
	Map_gomoku map(_game);

	points_branch(_color, place, map, cpt_eat);
}

coor	IAGomoku::random_place(uint64_t *map, int x, int y)
{
	std::list<coor>	places;
	int			rd;
	coor			fail;
	//std::srand(std::time(0));
	// std::random_device    _rd;
	//std::mt19937		gen(_rd());
	//std::default_random_engine generator;

	places = choose_places(map, x, y);
	//std::cout << "PLACE => " << places.size() << std::endl;
	//std::uniform_int_distribution<> dis(0, places.size() - 1);
	std::uniform_int_distribution<int> distribution(0, places.size() - 1);
	if (places.size() == 0)
	{
		fail.x = -1;
		return (fail);
	}
	rd = 0;
	/* for (int i = 0; i < 5; i++)
	{
	rd = distribution(generator);
	std::cout << "rd = " << rd << std::endl;
	}*/
	//rd = 0;
	//rd = std::rand()%(places.size() + 1) ;
	rd = distribution(_generator);
	for (int i = 0; i < rd; i++)
	{
		places.pop_front();
	}
	return (places.front());
}

void	IAGomoku::points_branch(int color, coor place, Map_gomoku &map, coor cpt_eat)
{
	int	cpt;

	if (place.x == -1)
		return;
	map.setPiece(place.x, place.y, color);
	_depth = _depth - 1;
	cpt = eat(map, place);
	if (cpt != 0)
	{
		if (color == _color)
		{
			_points += HEUR_EAT;
			cpt_eat.x += cpt;
			if (cpt_eat.x >= 10 && cpt_eat.x - cpt < 10)
				_points += HEUR_WIN;
		}
		else
		{
			_points += HEUR_EATEN;
			cpt_eat.y += cpt;
			if (cpt_eat.y >= 10 && cpt_eat.y - cpt < 10)
				_points += HEUR_LOSE;
		}
	}
	if (cpt > 0 && _rule_brk == true)
		if ((cpt = check_5_align_board(map)) != EMPTY)
			_points += (cpt == color) ? (HEUR_WIN) : (HEUR_LOSE);
	_points += check_win(map, place);
	color = (color == BLACK) ? (WHITE) : (BLACK);
	if (_depth > 0)
	{
		points_branch(color, random_place(map.getMap(), place.x, place.y), map, cpt_eat);
	}
}

int	IAGomoku::color_win_branch(int color, coor place, Map_gomoku &map, coor cpt_eat)
{
	int			res;
	int			cpt;

	if (place.x == -1)
		return (ELSE);
	map.setPiece(place.x, place.y, color);
	cpt = eat(map, place);
	if (color == WHITE)
		cpt_eat.x += cpt;
	else
		cpt_eat.y += cpt;
	if (cpt_eat.x >= 10)
		return (WHITE);
	if (cpt_eat.y >= 10)
		return (BLACK);
	if (cpt > 0 && _rule_brk == true)
		if ((cpt = check_5_align_board(map)) != EMPTY)
			return (cpt);
	//map.printMap();
	//std::cout << place.x << ", " << place.y << std::endl;
	//usleep(50000);
	color = (color == BLACK) ? (WHITE) : (BLACK);
	if ((res = check_win(map, place)) != EMPTY)
		return (res);
	return (color_win_branch(color, random_place(map.getMap(), place.x, place.y), map, cpt_eat));
}

int	IAGomoku::check_win(const Map_gomoku &map, coor place) const
{
	int	values_x[] = DIRX;
	int	values_y[] = DIRY;
	int	points = 0;
	int res_tmp = 0;

	for (int i = 0; i < 4; i++)
	{
		res_tmp = check_var_align(map, place, values_x[i], values_y[i]);
		if (res_tmp == 2)
			points += HEUR_TWO;
		else if (res_tmp == 3)
			points += HEUR_THREE;
		else if (res_tmp == 4)
			points += HEUR_FOUR;
		else if (res_tmp >= 5)
			points += HEUR_WIN;
		}
	return (EMPTY);
}

int	IAGomoku::eat(Map_gomoku &map, coor place) const
{
	int	values_x[] = DIRX;
	int	values_y[] = DIRY;
	int	cpt = 0;

	for (int i = 0; i < 8; i++)
	{
		cpt += eat_dir(map, place, values_x[i], values_y[i]);
	}
	return (cpt);
}

int	IAGomoku::eat_dir(Map_gomoku &map, coor place, int x_inc, int y_inc) const
{
	int	line = 0;
	int	color = map.getPiece(place.x, place.y);
	coor	cur;

	for (int i = 0; i < 4; i++)
	{
		cur.x = place.x + x_inc * i;
		cur.y = place.y + y_inc * i;
		if (IS_OUT(cur.x, cur.y))
			line = line | (int)(ELSE << (2 * i));
		else
			line = line | (int)(map.getPiece(cur.x, cur.y) << (2 * i));
	}
	if (line == EAT_PAT1 || line == EAT_PAT2)
	{
		map.setPiece(place.x + x_inc, place.y + y_inc, EMPTY);
		map.setPiece(place.x + x_inc * 2, place.y + y_inc * 2, EMPTY);
		return (2);
	}
	return (0);
}

int	IAGomoku::check_5_align_board(const Map_gomoku &map) const
{
	int	values_x[] = DIRX;
	int	values_y[] = DIRY;
	coor	place;
	int	res = EMPTY;

	for (place.x = 0; place.x < MAP_H; place.x++)
	{
		for (place.y = 0; place.y < MAP_H; place.y++)
		{
			for (int i = 0; i < 4; i++)
			{
				if ((res = check_5_align(map, place, values_x[i], values_y[i])) != EMPTY)
					return (res);
			}
		}
	}
	return (res);
}

int IAGomoku::check_var_align(const Map_gomoku &map, coor place, int x_inc, int y_inc) const
{
	int	forwd_len = 0;
	int	bacwd_len = 0;
	int	color = map.getPiece(place.x, place.y);
	coor	cur;

	cur.x = place.x;
	cur.y = place.y;
	while (!IS_OUT(cur.x, cur.y) && map.getPiece(cur.x, cur.y) == color)
	{
		cur.x += x_inc;
		cur.y += y_inc;
		forwd_len += 1;
	}
	cur.x = place.x - x_inc;
	cur.y = place.y - y_inc;
	while (!IS_OUT(cur.x, cur.y) && map.getPiece(cur.x, cur.y) == color)
	{
		cur.x -= x_inc;
		cur.y -= y_inc;
		bacwd_len += 1;
	}
	return (forwd_len + bacwd_len);
}

int	IAGomoku::check_5_align(const Map_gomoku &map, coor place, int x_inc, int y_inc) const
{
	int	forwd_len = 0;
	int	bacwd_len = 0;
	int	color = map.getPiece(place.x, place.y);
	coor	cur;

	cur.x = place.x;
	cur.y = place.y;
	while (!IS_OUT(cur.x, cur.y) && map.getPiece(cur.x, cur.y) == color)
	{
		cur.x += x_inc;
		cur.y += y_inc;
		forwd_len += 1;
	}
	cur.x = place.x - x_inc;
	cur.y = place.y - y_inc;
	while (!IS_OUT(cur.x, cur.y) && map.getPiece(cur.x, cur.y) == color)
	{
		cur.x -= x_inc;
		cur.y -= y_inc;
		bacwd_len += 1;
	}
	if (forwd_len + bacwd_len >= 5)
	{
		if (_rule_brk == false)
			return (color);
		place.x = place.x - x_inc * bacwd_len;
		place.y = place.y - y_inc * bacwd_len;
		return (check_breakable(map, place, x_inc, y_inc));
	}
	return (EMPTY);
}

int	IAGomoku::check_breakable(const Map_gomoku &map, coor place, int x_inc, int y_inc) const
{
	int	values_x[] = DIRX;
	int	values_y[] = DIRY;
	int	cpt = 0;
	int	color = map.getPiece(place.x, place.y);
	coor	cur;

	cur.x = place.x;
	cur.y = place.y;
	while (!IS_OUT(cur.x, cur.y) && map.getPiece(cur.x, cur.y) == color)
	{
		cpt++;
		for (int i = 0; i < 8; i++)
			if (check_if_vulnerable(map, cur, values_x[i], values_y[i]) == true)
				cpt = 0;
		cur.x += x_inc;
		cur.y += y_inc;
	}
	if (cpt >= 5)
		return (color);
	return (EMPTY);
}

bool	IAGomoku::check_if_vulnerable(const Map_gomoku &map, coor place, int x_inc, int y_inc) const
{
	int	line;
	coor	cur;

	for (int dual = -2; dual < 0; dual++)
	{
		line = 0;
		for (int i = 0; i < 4; i++)
		{
			cur.x = place.x + (i + dual) * x_inc;
			cur.y = place.y + (i + dual) * y_inc;
			if (IS_OUT(cur.x, cur.y))
				line = line | (int)(ELSE << (i * 2));
			else
				line = line | (int)(map.getPiece(cur.x, cur.y) << (i * 2));
		}
		if (line == VUL_PAT1 || line == VUL_PAT2 || line == VUL_PAT3 || line == VUL_PAT4)
			return (true);
	}
	return (false);
}

void    IAGomoku::sort_score(std::vector<possibility> &m_list)
{
	possibility   x;
	int           j;

	for (int i = 1; i < m_list.size(); i++)
	{
		x = m_list[i];
		j = i - 1;
		while (j >= 0 && m_list[j].score < x.score)
		{
			std::swap(m_list[j + 1].score, m_list[j].score);
			std::swap(m_list[j + 1].pos.x, m_list[j].pos.x);
			std::swap(m_list[j + 1].pos.y, m_list[j].pos.y);
			j = j - 1;
		}
		std::swap(m_list[j + 1].score, x.score);
		std::swap(m_list[j + 1].pos.x, x.pos.x);
		std::swap(m_list[j + 1].pos.y, x.pos.y);
	}
}

