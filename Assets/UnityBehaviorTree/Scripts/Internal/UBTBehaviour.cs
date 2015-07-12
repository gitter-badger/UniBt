using UnityEngine;
using UBT;

public abstract class UBTBehaviour : MonoBehaviour
{
	private Brain _brain;
	
	public void Initialize(Brain brain)
	{
		this._brain = brain;
		Initialize();
	}
	
	protected abstract void Initialize();
	
	protected void FinishExecute(bool value)
	{
		_brain.FinishExecute(value);
	}
}
