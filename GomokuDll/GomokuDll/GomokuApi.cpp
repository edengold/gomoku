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

MYGOMOKU_API void Opt3Rule(GomokuApi *api)
{
	api->set3Rule(false);
}

MYGOMOKU_API void OptBreakRule(GomokuApi *api)
{
	api->set3BreakRule(false);
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
	_is3rule = true;
	_isBreakRule = true;
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

bool GomokuApi::check_5_align(int x, int y) const
{
	std::list<std::list<pair>>	lines;

	if (check_if_free_cst(pair(x, y)) == true)
		return false;
	for (int rotation = 0; rotation < 4; rotation++)
		lines.push_back(check_line_align((rotation + 2) / 3,
		(rotation == 0) ? (1) : (2 - rotation), x, y));
	for (std::list<std::list<pair>>::iterator i = lines.begin(); i != lines.end(); i++)
	{
		if (i->size() >= 5)
		{
			if (!_isBreakRule)
				return true;
			if (!check_line_breakable(*i))
				return (true);
		}
	}
	return (false);
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

bool GomokuApi::is_3_align(int x, int y, board::Direction dir, bool color) const
{
	pair                  inc = pair((static_cast<int>(dir) + 2) / 3,
		(static_cast<int>(dir) == 0) ? (1) :
		(2 - static_cast<int>(dir)));
	std::string           line;
	std::map<pair, bool>  board = get_board();

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
			else if (check_if_free_cst(pair(x, y)) == true)
				line.append("f");
			else
				line.append((_board.at(pair(x, y)) == color) ? ("s") : ("o"));
		}
	}
	if (line.find("fssCff") != std::string::npos || line.find("ffCssf") != std::string::npos
		|| line.find("fsCsff") != std::string::npos || line.find("ffsCsf") != std::string::npos
		|| line.find("fsfsCf") != std::string::npos || line.find("fCsfsf") != std::string::npos
		|| line.find("fsfCsf") != std::string::npos || line.find("fsCfsf") != std::string::npos
		|| line.find("fssfCf") != std::string::npos || line.find("fCfssf") != std::string::npos)
		return (true);
	return (false);
}

int    GomokuApi::check_pieces_taken(std::pair<int, int> one, std::pair<int, int> two, std::pair<int, int> three, int **tab)
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
	}
	return 2;
}
int **GomokuApi::check_if_can_take(std::pair<int, int> pos)
{
	int                 count;
	int                 **tab;

	count = 0;
	tab = new int*[20];
	for (int i = 0; i < 20; ++i)
		tab[i] = new int[2];
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
	int	cpt = 0;

	for (int dir = 0; dir < 4; dir++)
	{
		if (is_3_align(x, y, static_cast<board::Direction>(dir), color) == true)
			cpt++;
	}
	if (cpt >= 2)
		return (true);
	return (false);
}
#pragma endregion 

#pragma region ForCommunication

bool GomokuApi::CanIPutHere(int pos)
{
	int x = static_cast<int>(pos % 20);
	int y = static_cast<int>(pos / 20);
	//bool -> if i can put pion here 
	if (!is_double_3_align(x, y, _color))
	{
		//func to checkprise si oui ajouter au tab des pion a delete
		//check si victoire
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
		if (check_5_align(x, y))
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
	_is3rule = false;
}

void GomokuApi::set3BreakRule(bool val)
{
	_isBreakRule = false;
}

#pragma endregion 



void    GomokuApi::test()
{
	int **tab;
	put_piece(pair(0, 0), true);
	put_piece(pair(0, 2), true);
	put_piece(pair(1, 0), false);
	put_piece(pair(2, 0), false);

	_color = true;
	//tab = move(3, 0, true);
	std::cout << tab[0][0] << std::endl;
	std::cout << tab[0][1] << std::endl;
	std::cout << tab[1][0] << std::endl;
	std::cout << tab[1][1] << std::endl;
}
/*
int main()
{
	GomokuApi gomoku;
	//gomoku.test();
	gomoku.CanIPutHere(0);
	gomoku.CanIPutHere(1);
	gomoku.CanIPutHere(12);
	gomoku.CanIPutHere(2);
	gomoku.CanIPutHere(3);
	gomoku.CanIPutHere(407);
	gomoku.CanIPutHere(3);
	gomoku.CanIPutHere(400);
	gomoku.CanIPutHere(4);
	std::map<std::pair<int, int>, bool>	board = gomoku.get_board();
	for (std::map<std::pair<int, int>, bool>::iterator it = board.begin(); it != board.end(); ++it)
	{
		printf("x = %d y = %d =>", it->first.first, it->first.second);
		if (it->second)
			printf("blanc\n");
		else
		{
			printf("noir\n");
		}
		it->second; // accede à la valeur
	}
	if (gomoku.check_5_align(0,0))
	{
		printf("c aligner");
	}
	for (int i = 0; i < gomoku._deletedPion.size(); i++)
	{
		printf("%d ", gomoku._deletedPion[i]);
	}
	printf("\n%d ", gomoku.GetDeletedPion());
	printf("\n%d ", gomoku.GetDeletedPion());
	printf("\n%d ", gomoku.GetDeletedPion());
	while (true)
	{
		
	}
	return 0;
}*/