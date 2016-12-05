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
	int res = api->check_5_align_board();
	if (res == 0 || res == -1)
		return -1;
}

MYGOMOKU_API int GetError(GomokuApi *api)
{
	return api->getError();
}

MYGOMOKU_API void ChangeMap(GomokuApi *api, int x, int y, int color)
{
	if (color == 1)
		api->special_move(x, y, WHITE);
	else if (color == 0)
		api->special_move(x, y, BLACK);
	else
		api->special_move(x, y, EMPTY);
}

#pragma endregion

GomokuApi::GomokuApi()
{
	_color = WHITE;
	_isVictory = false;
	_victoryTeam = false;
	_is3rule = false;
	_isBreakRule = false;
	_error = 100;
}

GomokuApi::~GomokuApi()
{
}

#pragma region ForAPI

Map_gomoku GomokuApi::get_board() const
{
    return _board;
}

int GomokuApi::get_turn() const
{
    return _color;
}

void GomokuApi::set_turn() 
{
	if (_color == BLACK)
		_color = WHITE;
	else
	{
		_color = BLACK;
	}
}
/*
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
*/
int GomokuApi::put_piece(int x, int y, int color)
{
	if (color != _color)
		return false;
	_board.setPiece(x, y, color);
	if (_color == BLACK)
		_color = WHITE;
	else
		_color = BLACK;
	return true;
}
/*
bool GomokuApi::check_if_free(std::pair<int, int> pos) const
{
	if (_board.find(pos) == _board.end())
		return true;
	return false;
}
*/


bool GomokuApi::check_5_align(pair pos, std::list<pair> & pieces) const
{
	std::list<std::list<pair>>    lines;
	bool                          check = false;

	if (_board.isEmpty(pos.first, pos.second) == false)
	{
		for (int rotation = 0; rotation < 4; rotation++)
			lines.push_back(check_line_align((rotation + 2) / 3,
			(rotation == 0) ? (1) : (2 - rotation), pos.first, pos.second));
	}
	for (std::list<std::list<pair>>::iterator i = lines.begin(); i != lines.end(); i++)
	{
		for (std::list<pair>::iterator elem = i->begin(); elem != i->end(); elem++)
			pieces.push_back(*elem);
		if (i->size() >= 5) {
		//	if (!_isBreakRule)
			//	return true;
			if (check_line_breakable(*i) == false)
				check = true;
		}
	}
	return (check);
}

int GomokuApi::check_5_align_board()
{
	std::list<pair>       pieces;

	for (int x = 0; x < MAP_H; x++)
	{
		for (int y = 0; y < MAP_H; y++)
		{
			if (std::find(pieces.begin(), pieces.end(), pair(x, y)) == pieces.end()
				&& check_5_align(pair(x, y), pieces) == true)
			{
				_error = _board.getPiece(x, y);
				return (_board.getPiece(x, y));
			} 
		}
	}
	_error = -1;
	return -1;
}

std::list<pair> GomokuApi::check_line_align(int x_inc, int y_inc, int x, int y) const
{
	bool					color = (_board.getPiece(x, y) == WHITE) ? (true) : (false);
	std::list<std::pair<int, int>>        line;
	bool					cursor = color;
	if (_board.isEmpty(x, y) == true)
	{
		line.push_back(std::pair<int, int>(x, y));
		return line;
	}
	while (cursor == color)
	{
		x += x_inc;
		y += y_inc;
		cursor = (_board.isEmpty(x, y) == true) ? (!color) : ((_board.getPiece(x, y) == WHITE) ? (true) : (false));
	}
	x -= x_inc; y -= y_inc; cursor = color;
	while (cursor == color)
	{
		line.push_back(std::pair<int, int>(x, y));
		x -= x_inc;
		y -= y_inc;
		cursor = (_board.isEmpty(x, y) == true) ? (!color) : ((_board.getPiece(x, y) == WHITE) ? (true) : (false));
	}
	return (line);
}

bool GomokuApi::check_line_breakable(const std::list<pair> &list) const
{
	unsigned int          cpt = 1;
	std::list<pair>       vul_points_tmp;
	std::list<pair>       vul_points;

	for (std::list<pair>::const_iterator it = list.begin(); it != list.end(); it++)
	{
		if ((vul_points_tmp = check_if_vulnerable(it->first, it->second)).size() > 0)
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

std::list<pair> GomokuApi::check_if_vulnerable(int x, int y) const
{
	int                   x_inc, y_inc;
	int			color = _board.getPiece(x, y);
	std::string           pattern;
	std::list<pair>       vul_points;
	size_t                char_pos;

	for (int rotation = 0; rotation < 4; rotation++)
	{
		pattern = "";
		x_inc = (rotation + 2) / 3; y_inc = (rotation == 0) ? (1) : (2 - rotation);
		x -= 2 * x_inc; y -= 2 * y_inc;
		for (int i = 0; i < 5; i++)
		{
			if (check_if_out(x, y) == true)
				pattern.append("v");
			else if (_board.isEmpty(x, y) == true)
				pattern.append("f");
			else
				pattern.append((_board.getPiece(x, y) == color) ? ("s") : ("o"));
			x += x_inc; y += y_inc;
		}
		if ((char_pos = pattern.find("ossf")) != std::string::npos)
			vul_points.push_back(pair(x + (char_pos - 1) * x_inc, y + (char_pos - 1) * y_inc));
		if ((char_pos = pattern.find("fsso")) != std::string::npos)
		{
			vul_points.push_back(pair(x + (char_pos - 5) * x_inc, y + (char_pos - 5) * y_inc));
		}
		x -= 3 * x_inc; y -= 3 * y_inc;
	}
	return (vul_points);
}

bool GomokuApi::check_if_out(int x, int y) const
{
	if (x < 0 || x >= MAP_H || y < 0 || y >= MAP_H)
		return (true);
	return (false);
}

pair GomokuApi::is_3_align(int x, int y, int inc_x, int inc_y, int color) const
{
	std::string           line;
	size_t                ret = std::string::npos;
	size_t                tmp;

	x = x - 5 * inc_x;
	y = y - 5 * inc_y;
	for (int i = 0; i < 9; i++)
	{
		x += inc_x;
		y += inc_y;
		if (i == 4)
			line.append("C");
		else
		{
			if (check_if_out(x, y) == true)
				line.append("v");
			else if (_board.isEmpty(x, y) == true)
				line.append("f");
			else
				line.append((_board.getPiece(x, y) == color) ? ("s") : ("o"));
		}
	}
	if (PATTERN("fssCff")
		|| PATTERN("ffCssf")
		|| PATTERN("fsCsff")
		|| PATTERN("ffsCsf")
		|| PATTERN("fsfsCf")
		|| PATTERN("fCsfsf")
		|| PATTERN("fsfCsf")
		|| PATTERN("fsCfsf")
		|| PATTERN("fssfCf")
		|| PATTERN("fCfssf"))
		return (pair(x + (ret - 8) * inc_x, y + (ret - 8) * inc_y));
	return (pair(-1, -1));
}

int    GomokuApi::check_pieces_taken(std::pair<int, int> one, std::pair<int, int> two, std::pair<int, int> three, int **tab)
{
	if (_board.isEmpty(one.first, one.second) || _board.isEmpty(two.first, two.second) || _board.isEmpty(three.first, three.second))
		return 0;
	if (_board.getPiece(one.first, one.second) == _color && _board.getPiece(two.first, two.second) != _color && _board.getPiece(three.first, three.second) != _color)
	{
		tab[0][0] = two.first;
		tab[0][1] = two.second;
		_board.setPiece(two.first, two.second, EMPTY);
		tab[1][0] = three.first;
		tab[1][1] = three.second;
		_board.setPiece(three.first, three.second, EMPTY);
		return 2;
	}
	return 0;
}

int **GomokuApi::check_if_can_take(int x, int y)
{
	std::pair<int, int> pos(x, y);
	int                 count;
	int                 **tab;

	count = 0;
	tab = new int*[MAP_H];
	for (int i = 0; i < MAP_H; ++i)
		tab[i] = new int[2];
	count += check_pieces_taken(pair(pos.first + 3, pos.second), pair(pos.first + 2, pos.second), pair(pos.first + 1, pos.second), &tab[count]);
	count += check_pieces_taken(pair(pos.first - 3, pos.second), pair(pos.first - 2, pos.second), pair(pos.first - 1, pos.second), &tab[count]);
	count += check_pieces_taken(pair(pos.first, pos.second + 3), pair(pos.first, pos.second + 2), pair(pos.first, pos.second + 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first, pos.second - 3), pair(pos.first, pos.second - 2), pair(pos.first, pos.second - 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first + 3, pos.second + 3), pair(pos.first + 2, pos.second + 2), pair(pos.first + 1, pos.second + 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first - 3, pos.second - 3), pair(pos.first - 2, pos.second - 2), pair(pos.first - 1, pos.second - 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first - 3, pos.second + 3), pair(pos.first - 2, pos.second + 2), pair(pos.first - 1, pos.second + 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first + 3, pos.second - 3), pair(pos.first + 2, pos.second - 2), pair(pos.first + 1, pos.second - 1), &tab[count]);
	tab[count][0] = -1;
	return tab;
}

bool    GomokuApi::is_double_3_align(int x, int y, int color) const
{
	pair  inc;
	pair  inc_2;
	pair  start;
	pair  cur;
	if (!_is3rule)
		return false;
	for (int dir = 0; dir < 4; dir++)
	{
		inc = pair((dir + 2) / 3, (dir == 0) ? (1) : (2 - dir));
		if ((start = is_3_align(x, y, inc.first, inc.second, color)) != pair(-1, -1))
		{
			for (int i = 0; i < 5; i++)
			{
				cur = pair(start.first + inc.first * i, start.second + inc.second * i);
				if ((_board.isEmpty(cur.first, cur.second) == false && _board.getPiece(cur.first, cur.second) == color) || (cur.first == x && cur.second == y))
				{
					for (int dir_2 = (dir + 1) % 4; dir_2 % 4 != dir; dir_2 = (dir_2 + 1) % 4)
					{
						inc_2 = pair((dir_2 + 2) / 3, (dir_2 == 0) ? (1) : (2 - dir_2));
						if (is_3_align(cur.first, cur.second, inc_2.first, inc_2.second, color) != pair(-1, -1))
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
	//	std::pair<int, int> tmp(x, y);
		put_piece(x ,y, _color);
		int                 **tab;

		tab = check_if_can_take(x, y);
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
		if (check_5_align_board() != -1 && check_5_align_board() != 0)
		{
			_isVictory = true;
			_victoryTeam = _color;
			return true;
		}
		//_color = !_color;
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
	//std::pair<int, int> pos(x, y);

	_board.setPiece(x, y, color);
/*	if (color >= 0)
		_board[pos] = color;
	else
		_board.erase(pos);*/
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