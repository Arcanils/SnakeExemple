using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseFood : IBaseEntity
{
	private RectTransform m_rtRef;

	public int XPosition { get; private set; }
	public int YPosition { get; private set; }

	public BaseFood(RectTransform rt)
	{
		m_rtRef = rt;
	}
	EEatResult IBaseEntity.Eat()
	{
		return EEatResult.AddSnakePart;
	}

	void IBaseEntity.ManualUpdate()
	{

	}
	public void Move(Vector2 position, int xPosition, int yPosition)
	{
		m_rtRef.anchoredPosition = position;
		XPosition = xPosition;
		YPosition = yPosition;
	}
}
