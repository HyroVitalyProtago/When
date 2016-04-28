using Leap;

public class HandEnableDisable : HandTransitionBehavior {

	protected override void HandReset() {
		gameObject.SetActive(true);
	}
    
	protected override void HandFinish() {
		gameObject.SetActive(false);
    }
	
}
