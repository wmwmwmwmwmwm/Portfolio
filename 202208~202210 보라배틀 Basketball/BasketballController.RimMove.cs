using UnityEngine;

namespace BoraBattle.Game.WorldBasketballKing
{
	public partial class BasketballController
	{
		public enum eMoveKind
		{
			eMoveKind_None,
			eMoveKind_1,
			eMoveKind_2,
			eMoveKind_3,
			eMoveKind_4,
			eMoveKind_5,
			eMoveKind_6
		}
		eMoveKind _moveKind;

		int _moveCount;
		int _actionType;

		Vector3 saveCenter;
		int _moveDirection;
		int _randomKind;
		bool _clockDirection;

		Transform _rim, _rimCenter;

		bool _nextActionReady;
		float _saveTime;
		float _saveYisPlus;
		float _testAngle = 0.0f;
		float fRimSpeed = 0.4f;

		void RimUpdate()
		{
			switch (_moveKind)
			{
				case eMoveKind.eMoveKind_None:
					break;
				case eMoveKind.eMoveKind_1:
					saveCenter = _rimCenter.position;
					Move_one();
					break;
				case eMoveKind.eMoveKind_2:
					saveCenter = _rimCenter.position;
					Move_two();
					break;
				case eMoveKind.eMoveKind_3:
					saveCenter = _rimCenter.position;
					Move_three();
					break;
				case eMoveKind.eMoveKind_4:
					saveCenter = _rimCenter.position;
					Move_four();
					break;
				case eMoveKind.eMoveKind_5:
					saveCenter = _rimCenter.position;
					Move_five();
					break;
				case eMoveKind.eMoveKind_6:
					saveCenter = _rimCenter.position;
					Move_six();
					break;
			}
		}

		void Move_one()
		{
			Vector3 moveVec = Vector3.zero;

			if (_actionType == 0)
			{ // 처음에는 움직임 없다.
				if (_nextActionReady)
				{
					_nextActionReady = false;
					_moveDirection = (int)randomProvider.Next(0, 2);
					_actionType = 1;
				}
			}
			else if (_actionType == 1)
			{
				if (_moveDirection == 0)
					moveVec.x += 2.0f;
				else
					moveVec.x -= 2.0f;

				_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);

				if (_rim.position.x >= saveCenter.x + 1.2f)
				{
					_moveDirection = 1;
				}
				else if (_rim.position.x <= saveCenter.x - 1.2f)
					_moveDirection = 0;
				if (_rim.position.x <= saveCenter.x + 0.01f && _rim.position.x >= saveCenter.x - 0.01f)
				{
					_rim.position = saveCenter;
				}
			}
		}

		void Move_two()
		{
			Vector3 moveVec = Vector3.zero;
			if (_actionType == 0)
			{ // 처음에는 움직임 없다.
				if (_nextActionReady)
				{
					_nextActionReady = false;
					_moveDirection = (int)randomProvider.Next(0, 2);
					_actionType = 1;
				}
			}
			else if (_actionType == 1)
			{ // 액션타입1은 가로로 움직인다.

				if (_moveDirection == 0)
					moveVec.x += 2.0f;
				else
					moveVec.x -= 2.0f;

				_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);

				if (_rim.position.x >= saveCenter.x + 1.2f)
				{
					_moveDirection = 1;
				}
				else if (_rim.position.x <= saveCenter.x - 1.2f)
					_moveDirection = 0;

				if (_rim.position.x <= saveCenter.x + 0.01f && _rim.position.x >= saveCenter.x - 0.01f)
				{
					_moveCount += 1;
					_rim.position = saveCenter;

					if (_nextActionReady)
					{
						_moveDirection = (int)randomProvider.Next(0, 2);
						_nextActionReady = false;
						_actionType = 2;
					}
				}
			}
			else if (_actionType == 2)
			{ // 액션타입2는 세로로 움직인다.

				if (_moveDirection == 0)
					moveVec.y += 2.0f;
				else
					moveVec.y -= 2.0f;

				_rim.Translate(moveVec * Time.deltaTime * (fRimSpeed * 0.5f), Space.World);

				if (_rim.position.y >= saveCenter.y + 0.6f)
				{
					_moveDirection = 1;
				}
				else if (_rim.position.y <= saveCenter.y - 0.6f)
					_moveDirection = 0;

				if (_rim.position.y <= saveCenter.y + 0.005f && _rim.position.y >= saveCenter.y - 0.005f)
				{
					_moveCount += 1;
					_rim.position = saveCenter;
					if (_nextActionReady)
					{
						_moveDirection = (int)randomProvider.Next(0, 2);
						_nextActionReady = false;
						_randomKind = (int)randomProvider.Next(0, 2);
						_actionType = 3;
					}
				}
			}
			else if (_actionType == 3)
			{// 액션타입3은 가로&세로(랜덤)로 움직인다.

				if (_randomKind == 0)
				{ // 가로방향

					if (_moveDirection == 0)
						moveVec.x += 2.0f;
					else
						moveVec.x -= 2.0f;

					_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);

					if (_rim.position.x >= saveCenter.x + 1.2f)
					{
						_moveDirection = 1;
					}
					else if (_rim.position.x <= saveCenter.x - 1.2f)
						_moveDirection = 0;

					if (_rim.position.x <= saveCenter.x + 0.01f && _rim.position.x >= saveCenter.x - 0.01f)
					{
						_moveCount += 1;
						_rim.position = saveCenter;
					}

					if (_moveCount >= 3)
					{
						_moveCount = 0;
						_moveDirection = (int)randomProvider.Next(0, 2);
						_randomKind = (int)randomProvider.Next(0, 2);
					}
				}
				else if (_randomKind == 1)
				{ // 세로 방향

					if (_moveDirection == 0)
						moveVec.y += 2.0f;
					else
						moveVec.y -= 2.0f;

					_rim.Translate(moveVec * Time.deltaTime * (fRimSpeed * 0.5f), Space.World);

					if (_rim.position.y >= saveCenter.y + 0.6f)
					{
						_moveDirection = 1;
					}
					else if (_rim.position.y <= saveCenter.y - 0.6f)
						_moveDirection = 0;

					if (_rim.position.y <= saveCenter.y + 0.005f && _rim.position.y >= saveCenter.y - 0.005f)
					{
						_moveCount += 1;
						_rim.position = saveCenter;
					}

					if (_moveCount >= 3)
					{
						_moveCount = 0;
						_moveDirection = (int)randomProvider.Next(0, 2);
						_randomKind = (int)randomProvider.Next(0, 2);
					}
				}
			}
		}

		void Move_three()
		{
			Vector3 moveVec = Vector3.zero;

			if (_actionType == 0)
			{

				if (_nextActionReady)
				{
					_nextActionReady = false;
					_moveDirection = (int)randomProvider.Next(0, 2);
					_actionType = 1;
				}
			}
			else if (_actionType == 1)
			{ // 가로 이

				if (_moveDirection == 0)
					moveVec.x += 2.0f;
				else
					moveVec.x -= 2.0f;

				_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);

				if (_rim.position.x >= saveCenter.x + 1.2f)
				{
					_moveDirection = 1;
				}
				else if (_rim.position.x <= saveCenter.x - 1.2f)
					_moveDirection = 0;

				if (_rim.position.x <= saveCenter.x + 0.01f && _rim.position.x >= saveCenter.x - 0.01f)
				{
					_moveCount += 1;
					_rim.position = saveCenter;

					if (_nextActionReady)
					{

						do
						{
							_moveDirection = (int)randomProvider.Next(0, 3);
						} while (_moveDirection == 1);

						_nextActionReady = false;
						_actionType = 2;
					}
				}
			}
			else if (_actionType == 2)
			{ //n모양 이동

				if (_moveDirection == 0)
					moveVec.x += 2.0f;
				else if (_moveDirection == 1)
					moveVec.y += 2.0f;
				else if (_moveDirection == 2)
					moveVec.x -= 2.0f;
				else if (_moveDirection == 3)
					moveVec.y -= 2.0f;

				_rim.Translate(moveVec * Time.deltaTime * (fRimSpeed * 0.917f), Space.World);


				if (_rim.position.x >= saveCenter.x + 1.2f)
				{
					if (_moveDirection == 0)
						_moveDirection = 3;
				}
				else if (_rim.position.x <= saveCenter.x - 1.2f)
				{
					if (_moveDirection == 2)
						_moveDirection = 3;
				}

				if (_rim.position.y >= saveCenter.y + 1.0f)
				{
					if (_moveDirection == 1 && !_clockDirection)
						_moveDirection = 2;
					if (_moveDirection == 1 && _clockDirection)
						_moveDirection = 0;

				}
				else if (_rim.position.y <= saveCenter.y)
				{
					if (_moveDirection == 3)
					{
						_moveDirection = 1;
						if (_clockDirection)
							_clockDirection = false;
						else
							_clockDirection = true;

						if (_nextActionReady)
						{
							_nextActionReady = false;
							_randomKind = (int)randomProvider.Next(0, 2);
							_actionType = 3;
						}
					}
				}
			}
			else if (_actionType == 3)
			{
				bool isLeft = false;
				if (_randomKind == 0)
				{ // 가로 방향

					if (_moveDirection == 0)
						moveVec.x += 2.0f;
					else
						moveVec.x -= 2.0f;

					_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);

					if (_rim.position.x >= saveCenter.x + 1.2f)
					{
						_moveDirection = 1;
						_moveCount += 1;
						isLeft = false;
					}
					else if (_rim.position.x <= saveCenter.x - 1.2f)
					{
						_moveDirection = 0;
						_moveCount += 1;
						isLeft = true;
					}

					if (_moveCount >= 3)
					{ // random
						_moveCount = 0;
						_randomKind = (int)randomProvider.Next(0, 2);

						if (_randomKind == 1 && isLeft)
						{
							_moveDirection = 1;
							_clockDirection = true;
						}
						if (_randomKind == 1 && !isLeft)
						{
							_moveDirection = 1;
							_clockDirection = false;
						}
					}
					if (_rim.position.x <= saveCenter.x + 0.01f && _rim.position.x >= saveCenter.x - 0.01f)
					{
						_rim.position = saveCenter;
					}
				}
				else if (_randomKind == 1)
				{ // n모양 방

					if (_moveDirection == 0)
						moveVec.x += 2.0f;
					else if (_moveDirection == 1)
						moveVec.y += 2.0f;
					else if (_moveDirection == 2)
						moveVec.x -= 2.0f;
					else if (_moveDirection == 3)
						moveVec.y -= 2.0f;

					_rim.Translate(moveVec * Time.deltaTime * (fRimSpeed * 0.917f), Space.World);

					if (_rim.position.x >= saveCenter.x + 1.2f)
					{
						if (_moveDirection == 0)
							_moveDirection = 3;
					}
					else if (_rim.position.x <= saveCenter.x - 1.2f)
					{
						if (_moveDirection == 2)
							_moveDirection = 3;
					}

					if (_rim.position.y >= saveCenter.y + 1.0f)
					{
						if (_moveDirection == 1 && !_clockDirection)
							_moveDirection = 2;
						if (_moveDirection == 1 && _clockDirection)
							_moveDirection = 0;

					}
					else if (_rim.position.y <= saveCenter.y)
					{
						if (_moveDirection == 3)
						{
							_moveDirection = 1;
							if (_clockDirection)
								_clockDirection = false;
							else
								_clockDirection = true;

							_moveCount += 1;

							if (_moveCount >= 3)
							{
								_moveCount = 0;
								_randomKind = (int)randomProvider.Next(0, 2);

								if (_randomKind == 0 && _clockDirection)
								{
									_moveDirection = 0;
								}
								if (_randomKind == 0 && !_clockDirection)
								{
									_moveDirection = 1;
								}
							}
						}
					}
				}
			}
		}

		void Move_four()
		{
			Vector3 moveVec = Vector3.zero;
			if (_actionType == 0)
			{
				if (_nextActionReady)
				{
					_nextActionReady = false;
					_moveDirection = (int)randomProvider.Next(0, 2);
					_actionType = 1;
				}
			}
			else if (_actionType == 1)
			{
				if (_moveDirection == 0)
					moveVec.x += 2.0f;
				else
					moveVec.x -= 2.0f;

				_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);

				if (_rim.position.x >= saveCenter.x + 1.2f)
				{
					_moveDirection = 1;
				}
				else if (_rim.position.x <= saveCenter.x - 1.2f)
					_moveDirection = 0;

				if (_rim.position.x <= saveCenter.x + 0.01f && _rim.position.x >= saveCenter.x - 0.01f)
				{
					_moveCount += 1;
					_rim.position = saveCenter;

					if (_nextActionReady)
					{
						_moveCount = 0;
						_moveDirection = (int)randomProvider.Next(0, 4);
						_nextActionReady = false;
						_actionType = 2;
					}
				}
			}
			else if (_actionType == 2)
			{

				if (_moveDirection == 0)
				{
					moveVec.x += 2.0f;
					moveVec.y += 1.5f;
				}
				else if (_moveDirection == 1)
				{
					moveVec.x -= 2.0f;
					moveVec.y -= 1.5f;
				}
				else if (_moveDirection == 2)
				{
					moveVec.x -= 2.0f;
					moveVec.y += 1.5f;
				}
				else if (_moveDirection == 3)
				{
					moveVec.x += 2.0f;
					moveVec.y -= 1.5f;
				}

				_rim.Translate(moveVec * Time.deltaTime * (fRimSpeed), Space.World);

				if (_rim.position.x >= saveCenter.x + 1.2f)
				{
					if (_moveDirection == 0)
						_moveDirection = 1;
					else if (_moveDirection == 3)
						_moveDirection = 2;
				}

				else if (_rim.position.x <= saveCenter.x - 1.2f)
				{
					if (_moveDirection == 2)
						_moveDirection = 3;
					else if (_moveDirection == 1)
						_moveDirection = 0;
				}

				if (_rim.position.x <= saveCenter.x + 0.01f && _rim.position.x >= saveCenter.x - 0.01f)
				{
					_moveCount += 1;
					_rim.position = saveCenter;

					if (_moveCount >= 3)
					{
						_moveCount = 0;
						int saveDirection = _moveDirection;

						do
						{
							_moveDirection = (int)randomProvider.Next(0, 4);
						} while (saveDirection == _moveDirection);
					}

					if (_nextActionReady)
					{
						_moveCount = 0;
						_randomKind = (int)randomProvider.Next(0, 3);
						_nextActionReady = false;
						_actionType = 3;
					}
				}
			}
			else if (_actionType == 3)
			{

				if (_randomKind == 0)
				{   // 가로 방향

					if (_moveDirection == 0)
						moveVec.x += 2.0f;
					else
						moveVec.x -= 2.0f;

					_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);

					if (_rim.position.x >= saveCenter.x + 1.2f)
					{
						_moveDirection = 1;
					}
					else if (_rim.position.x <= saveCenter.x - 1.2f)
					{
						_moveDirection = 0;
					}

					if (_rim.position.x <= saveCenter.x + 0.01f && _rim.position.x >= saveCenter.x - 0.01f)
					{
						_moveCount += 1;
						_rim.position = saveCenter;
					}

					if (_moveCount >= 3)
					{ // random
						_moveCount = 0;
						_randomKind = (int)randomProvider.Next(0, 3);

						if (_randomKind == 1 || _randomKind == 2)
							_moveDirection = (int)randomProvider.Next(0, 2);
					}
				}
				else if (_randomKind == 1)
				{ // / 방향

					if (_moveDirection == 0)
					{
						moveVec.x += 2.0f;
						moveVec.y += 1.5f;
					}
					else if (_moveDirection == 1)
					{
						moveVec.x -= 2.0f;
						moveVec.y -= 1.5f;
					}
					_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);

					if (_rim.position.x >= saveCenter.x + 1.2f)
					{
						if (_moveDirection == 0)
							_moveDirection = 1;
					}

					else if (_rim.position.x <= saveCenter.x - 1.2f)
					{
						if (_moveDirection == 1)
							_moveDirection = 0;
					}

					if (_rim.position.x <= saveCenter.x + 0.01f && _rim.position.x >= saveCenter.x - 0.01f)
					{
						_moveCount += 1;
						_rim.position = saveCenter;

						if (_moveCount >= 3)
						{ // random
							_moveCount = 0;
							_randomKind = (int)randomProvider.Next(0, 3);

							if (_randomKind == 1 || _randomKind == 2)
								_moveDirection = (int)randomProvider.Next(0, 2);
						}
					}
				}
				else if (_randomKind == 2)
				{ // \ 방

					if (_moveDirection == 0)
					{
						moveVec.x -= 2.0f;
						moveVec.y += 1.5f;
					}
					else if (_moveDirection == 1)
					{
						moveVec.x += 2.0f;
						moveVec.y -= 1.5f;
					}
					_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);

					if (_rim.position.x >= saveCenter.x + 1.2f)
					{
						if (_moveDirection == 1)
							_moveDirection = 0;
					}

					else if (_rim.position.x <= saveCenter.x - 1.2f)
					{
						if (_moveDirection == 0)
							_moveDirection = 1;
					}

					if (_rim.position.x <= saveCenter.x + 0.01f && _rim.position.x >= saveCenter.x - 0.01f)
					{
						_moveCount += 1;
						_rim.position = saveCenter;

						if (_moveCount >= 3)
						{ // random
							_moveCount = 0;
							_randomKind = (int)randomProvider.Next(0, 3);

							if (_randomKind == 1 || _randomKind == 2)
								_moveDirection = (int)randomProvider.Next(0, 2);
						}
					}
				}
			}
		}

		void Move_five()
		{
			Vector3 moveVec = Vector3.zero;

			if (_actionType == 0)
			{
				if (_nextActionReady)
				{
					_nextActionReady = false;
					_moveDirection = (int)randomProvider.Next(0, 2);
					_actionType = 1;
				}
			}
			else if (_actionType == 1)
			{

				if (_moveDirection == 0)
					moveVec.x += 2.0f;
				else
					moveVec.x -= 2.0f;

				_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);

				if (_rim.position.x >= saveCenter.x + 1.2f)
				{ // 오른쪽 끝에 체크
					_moveDirection = 1;

					if (_nextActionReady)
					{
						_moveCount = 0;
						_moveDirection = (int)randomProvider.Next(2, 4);
						_nextActionReady = false;
						_saveTime = Time.deltaTime;
						_actionType = 2;
					}
				}
				else if (_rim.position.x <= saveCenter.x - 1.2f)
				{ // 왼쪽 끝 체 
					_moveDirection = 0;

					if (_nextActionReady)
					{
						_moveCount = 0;
						_moveDirection = (int)randomProvider.Next(0, 2);
						_nextActionReady = false;
						_saveTime = Time.deltaTime;
						_actionType = 2;
					}
				}

				if (_rim.position.x <= saveCenter.x + 0.01f && _rim.position.x >= saveCenter.x - 0.01f)
				{
					_rim.position = saveCenter;
				}
			}
			else if (_actionType == 2)
			{
				//float angle = Time.time - _saveTime;
				if (_moveDirection == 0)
				{ // 시계 왼쪽시작
					moveVec.x = Mathf.Cos(_testAngle * 1.5f) * 1.2f; // +
					moveVec.y = Mathf.Sin(_testAngle * 1.5f); // +
				}
				else if (_moveDirection == 1)
				{ // 반시계 왼쪽
					moveVec.x = Mathf.Cos(_testAngle * 1.5f) * 1.2f; // +
					moveVec.y = -Mathf.Sin(_testAngle * 1.5f); // -
				}
				else if (_moveDirection == 2)
				{ // 시계 오른쪽
					moveVec.x = -Mathf.Cos(_testAngle * 1.5f) * 1.2f; // -
					moveVec.y = -Mathf.Sin(_testAngle * 1.5f); // -
				}
				else if (_moveDirection == 3)
				{ // 반시계 오른
					moveVec.x = -Mathf.Cos(_testAngle * 1.5f) * 1.2f; // -
					moveVec.y = Mathf.Sin(_testAngle * 1.5f); // +
				}
				//saveCenter = _rim._rim.position;
				Vector3 transPosition = new Vector3(-moveVec.x, moveVec.y, 0.0f);
				_rim.position = transPosition;
				_testAngle += (0.004f / fRimSpeed);

				if (((_saveYisPlus >= 0 && moveVec.y < 0) ||
					(_saveYisPlus < 0 && moveVec.y > 0)) && moveVec.x < -0.5f)
				{ // rightEnd
					_moveCount += 1;

					if (_moveCount >= 3)
					{
						_moveCount = 0;
						int saveDirection = _moveDirection;
						_moveDirection = (int)randomProvider.Next(2, 4);
						_testAngle = 0.0f;
						if (saveDirection == _moveDirection)
							_saveYisPlus = moveVec.y;

						_saveTime = Time.time;
					}
					else
					{
						_saveYisPlus = moveVec.y;
					}
					if (_nextActionReady)
					{
						_moveCount = 0;
						_moveDirection = (int)randomProvider.Next(2, 5);
						_nextActionReady = false;
						_saveTime = Time.time;
						_testAngle = 0.0f;
						_actionType = 3;
					}

				}
				else if (((_saveYisPlus >= 0 && moveVec.y < 0) ||
					(_saveYisPlus < 0 && moveVec.y > 0)) && moveVec.x > 0.5f)
				{
					_moveCount += 1;

					if (_moveCount >= 3)
					{
						_moveCount = 0;
						_testAngle = 0.0f;
						int saveDirection = _moveDirection;
						_moveDirection = (int)randomProvider.Next(0, 2);

						if (saveDirection == _moveDirection)
							_saveYisPlus = moveVec.y;

						_saveTime = Time.time;
					}
					else
					{
						_saveYisPlus = moveVec.y;
					}
					if (_nextActionReady)
					{
						_moveCount = 0;

						do
						{
							_moveDirection = (int)randomProvider.Next(0, 6);
						} while (_moveDirection == 2 || _moveDirection == 3 || _moveDirection == 4);

						_nextActionReady = false;
						_saveTime = Time.time;
						_testAngle = 0.0f;
						_actionType = 3;
					}
				}
			}
			else if (_actionType == 3)
			{
				if (_moveDirection == 0)
				{ // 시계 왼쪽시작
					moveVec.x = Mathf.Cos(_testAngle * 1.5f) * 1.2f; // +
					moveVec.y = Mathf.Sin(_testAngle * 1.5f); // +
				}
				else if (_moveDirection == 1)
				{ // 반시계 왼쪽
					moveVec.x = Mathf.Cos(_testAngle * 1.5f) * 1.2f; // +
					moveVec.y = -Mathf.Sin(_testAngle * 1.5f); // -
				}
				else if (_moveDirection == 2)
				{ // 시계 오른쪽
					moveVec.x = -Mathf.Cos(_testAngle * 1.5f) * 1.2f; // -
					moveVec.y = -Mathf.Sin(_testAngle * 1.5f); // -
				}
				else if (_moveDirection == 3)
				{ // 반시계 오른
					moveVec.x = -Mathf.Cos(_testAngle * 1.5f) * 1.2f; // -
					moveVec.y = Mathf.Sin(_testAngle * 1.5f); // +
				}
				else if (_moveDirection == 4)
				{
					moveVec.x -= 2.0f;
					moveVec.y = 0.0f;
				}
				else if (_moveDirection == 5)
				{
					moveVec.x += 2.0f;
					moveVec.y = 0.0f;
				}

				if (_moveDirection <= 3)
				{ // 회전
					Vector3 transPosition = new Vector3(saveCenter.x - moveVec.x, saveCenter.y + moveVec.y, saveCenter.z);
					_rim.position = transPosition;
					_testAngle += (0.004f / fRimSpeed);
					if (((_saveYisPlus >= 0 && moveVec.y < 0) ||
						(_saveYisPlus < 0 && moveVec.y > 0)) && moveVec.x < -0.5f)
					{
						_moveCount += 1;

						if (_moveCount >= 3)
						{
							_moveCount = 0;
							_testAngle = 0.0f;
							_saveTime = Time.time;
							int saveDirection = _moveDirection;
							_moveDirection = (int)randomProvider.Next(2, 5);
							//Debug.Log("11111 //" + _moveDirection);
							if (saveDirection == _moveDirection)
								_saveYisPlus = moveVec.y;
						}
						else
						{
							_saveYisPlus = moveVec.y;
						}
					}
					else if (((_saveYisPlus >= 0 && moveVec.y < 0) ||
						(_saveYisPlus < 0 && moveVec.y > 0)) && moveVec.x > 0.5f)
					{
						_moveCount += 1;
						if (_moveCount >= 3)
						{
							_moveCount = 0;
							_testAngle = 0.0f;
							int saveDirection = _moveDirection;
							do
							{
								_moveDirection = (int)randomProvider.Next(0, 6);
							} while (_moveDirection == 2 || _moveDirection == 3 || _moveDirection == 4);
							//Debug.Log("2222 //" + _moveDirection);
							if (saveDirection == _moveDirection)
								_saveYisPlus = moveVec.y;

							_saveTime = Time.time;
						}
						else
						{
							_saveYisPlus = moveVec.y;
						}
					}
				}
				else if (_moveDirection <= 5)
				{ // 가로방향
					_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);
					//saveCenter = _rim._rim.position;
					if (_rim.position.x >= saveCenter.x + 1.2f)
					{
						_moveDirection = 4;
						_moveCount += 1;

						if (_moveCount >= 3)
						{
							_moveCount = 0;
							_saveTime = Time.time;
							int saveDirection = _moveDirection;
							_moveDirection = (int)randomProvider.Next(2, 5);
							//Debug.Log ("testt " + _moveDirection); 

							if (saveDirection == _moveDirection)
								_saveYisPlus = moveVec.y;

						}
						else
						{
							_saveYisPlus = moveVec.y;
						}

					}
					else if (_rim.position.x <= saveCenter.x - 1.2f)
					{
						_moveDirection = 5;
						_moveCount += 1;

						if (_moveCount >= 3)
						{
							_moveCount = 0;
							int saveDirection = _moveDirection;
							do
							{
								_moveDirection = (int)randomProvider.Next(0, 6);
							} while (_moveDirection == 2 || _moveDirection == 3 || _moveDirection == 4);

							//Debug.Log ("test" + _moveDirection);
							if (saveDirection == _moveDirection)
								_saveYisPlus = moveVec.y;

							_saveTime = Time.time;
						}
						else
						{
							_saveYisPlus = moveVec.y;
						}
					}
				}
			}
		}

		void Move_six()
		{
			Vector3 moveVec = Vector3.zero;

			if (_actionType == 0)
			{

				if (_nextActionReady)
				{
					_nextActionReady = false;
					_moveDirection = (int)randomProvider.Next(0, 2);
					_actionType = 1;
				}

			}
			else if (_actionType == 1)
			{

				if (_moveDirection == 0)
					moveVec.x += 2.0f;
				else
					moveVec.x -= 2.0f;

				_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);
				//saveCenter = _rim._rim.position;
				if (_rim.position.x >= saveCenter.x + 1.2f)
				{ // 오른쪽 끝에 체크
					_moveDirection = 1;

					if (_nextActionReady)
					{
						_moveCount = 0;
						_moveDirection = (int)randomProvider.Next(0, 2);
						_nextActionReady = false;
						_saveTime = Time.deltaTime;
						_actionType = 2;
					}

				}
				else if (_rim.position.x <= saveCenter.x - 1.2f)
				{ // 왼쪽 끝 체 
					_moveDirection = 0;

					if (_nextActionReady)
					{
						_moveCount = 0;
						_moveDirection = (int)randomProvider.Next(0, 2);
						_nextActionReady = false;
						_saveTime = Time.deltaTime;
						_actionType = 2;
						//Debug.Log ("testStart");
					}

				}

				if (_rim.position.x <= saveCenter.x + 0.01f && _rim.position.x >= saveCenter.x - 0.01f)
				{
					_rim.position = saveCenter;
				}

			}
			else if (_actionType == 2)
			{

				if (_moveDirection == 0)
				{ //위로 
					moveVec.y += 2.0f;
					_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);

					if (_rim.position.y >= saveCenter.y + 1.2f && _rim.position.x >= 0.0f)
					{ // 오른쪽 위
						_moveDirection = 3;
					}

					if (_rim.position.y >= saveCenter.y + 1.2f && _rim.position.x <= 0.0f)
					{
						_moveDirection = 2;
					}

				}
				else if (_moveDirection == 1)
				{ //아래로
					moveVec.y -= 2.0f;
					_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);

					if (_rim.position.y <= saveCenter.y - 1.2f && _rim.position.x >= 0.0f)
					{ // 오른쪽 아래
						_moveDirection = 3;
					}

					if (_rim.position.y <= saveCenter.y - 1.2f && _rim.position.x <= 0.0f)
					{
						_moveDirection = 2;
					}

				}
				else if (_moveDirection == 2)
				{ // 오른쪽
					moveVec.x += 2.0f;
					_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);

					if (_rim.position.x >= saveCenter.x + 1.2f && _rim.position.y >= 0.0f)
					{ // 오론쪽 위
						_moveDirection = 1;
					}

					if (_rim.position.x >= saveCenter.x + 1.2f && _rim.position.y <= 0.0f)
					{ // 오른쪽 아래
						_moveDirection = 0;
					}

				}
				else if (_moveDirection == 3)
				{ // 왼쪽
					moveVec.x -= 2.0f;
					_rim.Translate(moveVec * Time.deltaTime * fRimSpeed, Space.World);

					if (_rim.position.x <= saveCenter.x - 1.2f && _rim.position.y >= 0.0f)
					{
						_moveDirection = 1;
					}

					if (_rim.position.x <= saveCenter.x - 1.2f && _rim.position.y < 0.0f)
					{
						_moveDirection = 0;
					}
				}
			}
			else if (_actionType == 3)
			{
			}
		}
	}
}