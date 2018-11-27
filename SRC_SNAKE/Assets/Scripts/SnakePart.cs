using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakePart : IBaseEntity
{
	private readonly bool m_isHead;
	private readonly RectTransform m_uiRef;

	public int XPosition { get; private set; }
	public int YPosition { get; private set; }

	public SnakePart(bool isHead, RectTransform uiRef)
	{
		m_isHead = isHead;
		m_uiRef = uiRef;
	}

	public void Move(Vector2 position, int xPosition, int yPosition)
	{
		m_uiRef.anchoredPosition = position;
		XPosition = xPosition;
		YPosition = yPosition;
	}

	EEatResult IBaseEntity.Eat()
	{
		return EEatResult.Death;
	}

	void IBaseEntity.ManualUpdate()
	{
		throw new NotImplementedException();
	}
}
