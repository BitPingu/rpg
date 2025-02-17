using UnityEngine;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour
{
    public Gradient _gradient;

    public void SetMaxHealth(float health)
    {
        GetComponent<Slider>().maxValue = health;
        GetComponent<Slider>().value = health;
        
        transform.Find("Fill").GetComponent<Image>().color = _gradient.Evaluate(1f);
    }

    public void SetHealth(float health)
    {
        GetComponent<Slider>().value = health;

        transform.Find("Fill").GetComponent<Image>().color = _gradient.Evaluate(GetComponent<Slider>().normalizedValue);
    }
}
