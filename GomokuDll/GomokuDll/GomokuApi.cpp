#include "GomokuApi.h"

#pragma region ForDll
MYGOMOKU_API int Add(int a, int b)
{
	return a + b;
}

MYGOMOKU_API GomokuApi* CreateGomokuAPI()
{
	return new GomokuApi();
}

MYGOMOKU_API void DeleteGomokuAPI(GomokuApi *api)
{
	delete api;
}

MYGOMOKU_API bool GetTurn(GomokuApi *api)
{
	return api->get_turn();
}

MYGOMOKU_API void SetTurn(GomokuApi *api)
{
	api->set_turn();
}

MYGOMOKU_API bool CanIPutHere(GomokuApi *api, int pos)
{
	return api->CanIPutHere(pos);
}

MYGOMOKU_API int GetDeletedPion(GomokuApi *api)
{
	return api->GetDeletedPion();
}

MYGOMOKU_API bool GetVictoryTeam(GomokuApi *api)
{
	return api->GetVictoryTeam();
}

MYGOMOKU_API bool GetVictory(GomokuApi *api)
{
	return  api->GetVictory();
}

MYGOMOKU_API void Opt3Rule(GomokuApi *api, bool val)
{
	api->set3Rule(val);
}

MYGOMOKU_API int OptBreakRule(GomokuApi *api, bool val)
{
	api->set3BreakRule(val);
	return api->check_5_align_board();
}

MYGOMOKU_API void ChangeMap(GomokuApi *api, int x, int y, int color)
{
	if (color == 1)
		api->special_move(x, y, 0);
	else if (color == 0)
		api->special_move(x, y, 1);
	else
		api->special_move(x, y, color);
}

#pragma endregion

GomokuApi::GomokuApi()
{
	_color = true;
	_isVictory = false;
	_victoryTeam = false;
	_is3rule = false;
	_isBreakRule = false;
}

GomokuApi::~GomokuApi()
{
}

#pragma region ForAPI

std::map<std::pair<int, int>, bool> GomokuApi::get_board() const
{
    return _board;
}

bool GomokuApi::get_turn() const
{
    return _color;
}

void GomokuApi::set_turn() 
{
	_color = !_color;
}

bool GomokuApi::check_if_free(std::pair<int, int> pos)
{
	if (_board.find(pos) == _board.end())
		return true;
	return false;
}

bool GomokuApi::check_if_free_cst(std::pair<int, int> pos) const
{
	if (_board.find(pos) == _board.end())
		return true;
	return false;
}

bool GomokuApi::put_piece(std::pair<int, int> pos, bool color)
{
    _board[pos] = color;
    return true;
}

bool GomokuApi::check_if_free(std::pair<int, int> pos) const
{
	if (_board.find(pos) == _board.end())
		return true;
	return false;
}

bool GomokuApi::check_5_align(pair pos, std::list<pair> & pieces) const
{
	std::list<std::list<pair>>	lines;
	bool				check = false;

	if (check_if_free(pos) == false)
	{
		for (int rotation = 0; rotation < 4; rotation++)
			lines.push_back(check_line_align((rotation + 2) / 3,
			(rotation == 0) ? (1) : (2 - rotation), pos.first, pos.second));
	}
	for (std::list<std::list<pair>>::iterator i = lines.begin(); i != lines.end(); i++)
	{
		for (std::list<pair>::iterator elem = i->begin(); elem != i->end(); elem++)
			pieces.push_back(*elem);
		if (i->size() >= 5)
		{
			if (!_isBreakRule)
				return true;
			if (check_line_breakable(*i) == false)
				check = true;
		}
	}
	return (check);
}

int GomokuApi::check_5_align_board() const
{
	std::list<pair>	pieces;

	for (std::map<pair, bool>::const_iterator it = _board.begin(); it != _board.end(); it++)
	{
		if (std::find(pieces.begin(), pieces.end(), it->first) == pieces.end()
			&& check_5_align(it->first, pieces) == true)
		{
			std::cout << "ret" << std::endl;
			for (std::list<pair>::const_iterator i = pieces.begin(); i != pieces.end(); i++)
				std::cout << "ok" << std::endl;
			return (static_cast<int>(_board.at(it->first)));
		}
	}
	return -1;
}

std::list<pair> GomokuApi::check_line_align(int x_inc, int y_inc, int x, int y) const
{
	std::map<std::pair<int, int>, bool>   board = get_board();
	bool                                  color = board[std::pair<int, int>(x, y)];
	std::list<std::pair<int, int>>        line;
	bool                                  cursor = color;

	while (cursor == color)
	{
		x += x_inc;
		y += y_inc;
		cursor = (check_if_free_cst(pair(x, y)) == true) ? (!color) : (board[pair(x, y)]);
	}
	x -= x_inc; y -= y_inc; cursor = color;
	while (cursor == color)
	{
		line.push_back(std::pair<int, int>(x, y));
		x -= x_inc;
		y -= y_inc;
		cursor = (check_if_free_cst(pair(x, y)) == true) ? (!color) : (board[pair(x, y)]);
	}
	return (line);
}

bool GomokuApi::check_line_breakable(std::list<pair> list) const
{
	unsigned int		cpt = 1;
	std::list<pair>	vul_points_tmp;
	std::list<pair>	vul_points;

	for (std::list<pair>::iterator it = list.begin(); it != list.end(); it++)
	{
		if ((vul_points_tmp = check_if_vulnerable(*it)).size() > 0)
		{
			if (((list.size() - cpt) < cpt) ? (cpt) : (list.size() - cpt) < 5)
				vul_points.splice(vul_points.begin(), vul_points_tmp);
		}
		cpt++;
	}
	if (vul_points.size() > 0)
		return (true);
	return (false);
}

std::list<pair> GomokuApi::check_if_vulnerable(pair pos) const
{
	int			x_inc, y_inc;
	bool			color = _board.at(pos);
	std::string		pattern;
	std::list<pair>	vul_points;
	size_t		char_pos;

	for (int rotation = 0; rotation < 4; rotation++)
	{
		pattern = "";
		x_inc = (rotation + 2) / 3; y_inc = (rotation == 0) ? (1) : (2 - rotation);
		pos.first -= 2 * x_inc; pos.second -= 2 * y_inc;
		for (int i = 0; i < 5; i++)
		{
			if (check_if_out(pos) == true)
				pattern.append("v");
			else if (check_if_free_cst(pos) == true)
				pattern.append("f");
			else
				pattern.append((_board.at(pos) == color) ? ("s") : ("o"));
			pos.first += x_inc; pos.second += y_inc;
		}
		if ((char_pos = pattern.find("ossf")) != std::string::npos)
			vul_points.push_back(pair(pos.first + (char_pos - 1) * x_inc, pos.second + (char_pos - 1) * y_inc));
		if ((char_pos = pattern.find("fsso")) != std::string::npos)
		{
			vul_points.push_back(pair(pos.first + (char_pos - 5) * x_inc, pos.second + (char_pos - 5) * y_inc));
		}
		pos.first -= 3 * x_inc; pos.second -= 3 * y_inc;
	}
	return (vul_points);
}

bool GomokuApi::check_if_out(pair coor) const
{
	if (coor.first < 0 || coor.first > 19 || coor.second < 0 || coor.second > 19)
		return (true);
	return (false);
}

pair GomokuApi::is_3_align(int x, int y, pair inc, bool color) const
{
	std::string           line;
	std::map<pair, bool>  board = get_board();
	size_t	        ret = std::string::npos;
	size_t		tmp;

	x = x - 5 * inc.first;
	y = y - 5 * inc.second;
	for (int i = 0; i < 9; i++)
	{
		x += inc.first;
		y += inc.second;
		if (i == 4)
			line.append("C");
		else
		{
			if (check_if_out(pair(x, y)) == true)
				line.append("v");
			else if (check_if_free(pair(x, y)) == true)
				line.append("f");
			else
				line.append((_board.at(pair(x, y)) == color) ? ("s") : ("o"));
		}
	}
	if ((ret = ((tmp = line.find("fssCff")) != std::string::npos) ? (tmp) : (ret)) != std::string::npos
		|| (ret = ((tmp = line.find("ffCssf")) != std::string::npos) ? (tmp) : (ret)) != std::string::npos
		|| (ret = ((tmp = line.find("fsCsff")) != std::string::npos) ? (tmp) : (ret)) != std::string::npos
		|| (ret = ((tmp = line.find("ffsCsf")) != std::string::npos) ? (tmp) : (ret)) != std::string::npos
		|| (ret = ((tmp = line.find("fsfsCf")) != std::string::npos) ? (tmp) : (ret)) != std::string::npos
		|| (ret = ((tmp = line.find("fCsfsf")) != std::string::npos) ? (tmp) : (ret)) != std::string::npos
		|| (ret = ((tmp = line.find("fsfCsf")) != std::string::npos) ? (tmp) : (ret)) != std::string::npos
		|| (ret = ((tmp = line.find("fsCfsf")) != std::string::npos) ? (tmp) : (ret)) != std::string::npos
		|| (ret = ((tmp = line.find("fssfCf")) != std::string::npos) ? (tmp) : (ret)) != std::string::npos
		|| (ret = ((tmp = line.find("fCfssf")) != std::string::npos) ? (tmp) : (ret)) != std::string::npos)
		return (pair(x + (ret - 8) * inc.first, y + (ret - 8) * inc.second));
	return (pair(-1, -1));
}

int  GomokuApi::check_pieces_taken(std::pair<int, int> one, std::pair<int, int> two, std::pair<int, int> three, int **tab)
{
	if (check_if_free(one) || check_if_free(two) || check_if_free(three))
		return 0;
	if (_board[one] == _color && _board[two] != _color && _board[three] != _color)
	{
		tab[0][0] = two.first;
		tab[0][1] = two.second;
		_board.erase(_board.find(two));
		tab[1][0] = three.first;
		tab[1][1] = three.second;
		_board.erase(_board.find(three));
		return 2;
	}
	return 0;
}

int **GomokuApi::check_if_can_take(std::pair<int, int> pos)
{
	int                 count;
	int                 **tab;

	count = 0;
	tab = new int*[20];
	for (int i = 0; i < 20; ++i)
	{
		tab[i] = new int[2];
		tab[i][0] = -1;
		tab[i][1] = -1;
	}
	count += check_pieces_taken(pair(pos.first + 3, pos.second), pair(pos.first + 2, pos.second), pair(pos.first + 1, pos.second), &tab[count]);
	count += check_pieces_taken(pair(pos.first - 3, pos.second), pair(pos.first - 2, pos.second), pair(pos.first - 1, pos.second), &tab[count]);
	count += check_pieces_taken(pair(pos.first, pos.second + 3), pair(pos.first, pos.second + 2), pair(pos.first, pos.second + 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first, pos.second - 3), pair(pos.first, pos.second - 2), pair(pos.first, pos.second - 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first + 3, pos.second + 3), pair(pos.first + 2, pos.second + 2), pair(pos.first + 1, pos.second + 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first - 3, pos.second - 3), pair(pos.first - 2, pos.second - 2), pair(pos.first - 1, pos.second - 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first - 3, pos.second + 3), pair(pos.first - 2, pos.second + 2), pair(pos.first - 1, pos.second + 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first + 3, pos.second - 3), pair(pos.first + 2, pos.second - 2), pair(pos.first + 1, pos.second - 1), &tab[count]);
	if (count == 0)
	{
		for (int i = 0; i < 20; ++i)
			delete tab[i];
		delete tab;
		return NULL;
	}
	tab[count][0] = -1;
	return tab;
}

bool	GomokuApi::is_double_3_align(int x, int y, bool color) const
{
	if (!_is3rule)
		return false;
	pair	inc;
	pair	inc_2;
	pair	start;
	pair	cur;

	for (int dir = 0; dir < 4; dir++)
	{
		inc = pair((static_cast<int>(dir) + 2) / 3,
			(static_cast<int>(dir) == 0) ? (1) : (2 - static_cast<int>(dir)));
		if ((start = is_3_align(x, y, inc, color)) != pair(-1, -1))
		{
			for (int i = 0; i < 5; i++)
			{
				cur = pair(start.first + inc.first * i, start.second + inc.second * i);
				if ((check_if_free(cur) == false && _board.at(cur) == color) || (cur.first == x && cur.second == y))
				{
					for (int dir_2 = (dir + 1) % 4; dir_2 % 4 != dir; dir_2 = (dir_2 + 1) % 4)
					{
						inc_2 = pair((static_cast<int>(dir_2) + 2) / 3,
							(static_cast<int>(dir_2) == 0) ?
							(1) : (2 - static_cast<int>(dir_2)));
						if (is_3_align(cur.first, cur.second, inc_2, color) != pair(-1, -1))
							return (true);
					}
				}
			}
		}
	}
	return (false);
}
#pragma endregion 

#pragma region ForCommunication

bool GomokuApi::CanIPutHere(int pos)
{
	int x = static_cast<int>(pos % 20);
	int y = static_cast<int>(pos / 20);
	if (!is_double_3_align(x, y, _color))
	{
		std::pair<int, int> tmp(x, y);
		put_piece(tmp, _color);
		int                 **tab;

		tab = check_if_can_take(tmp);
		if (tab != NULL && tab[0][0] != -1)
			{
				int i = 0;
				while (tab[i])
				{
					if (tab[i][0] == -1)
						break;
					if (tab[i][0] && tab[i][0] != -1)
					{
						_deletedPion.push_back((tab[i][1] * 20) + tab[i][0]);
					}
					i++;
				}
			}
		if (check_5_align_board() != -1)
		{
			_isVictory = true;
			_victoryTeam = _color;
			return true;
		}
		_color = !_color;
		return true;
	}
	return false;
}

int GomokuApi::GetDeletedPion()
{
	if (!_deletedPion.empty())
	{
		int tmp = _deletedPion.at(_deletedPion.size() - 1);
		_deletedPion.pop_back();
		return tmp;
	}
	return -1;
}

bool GomokuApi::GetVictoryTeam() const
{
	return _victoryTeam;
}

bool GomokuApi::GetVictory() const
{
	return _isVictory;
}

void GomokuApi::special_move(int x, int y, int color)
{
	std::pair<int, int> pos(x, y);

	if (color >= 0)
		_board[pos] = color;
	else
		_board.erase(pos);
}

void GomokuApi::set3Rule(bool val)
{
	_is3rule = val;
}

void GomokuApi::set3BreakRule(bool val)
{
	_isBreakRule = val;
}

#pragma endregion 