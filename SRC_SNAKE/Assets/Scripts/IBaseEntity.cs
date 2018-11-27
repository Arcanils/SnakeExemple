using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EEatResult
{
	AddSnakePart,
	Death,
}

public interface IBaseEntity
{
	void ManualUpdate();
	EEatResult Eat();
	void Move(Vector2 position, int xPosition, int yPosition);
}
