using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject healthBarParent = null;
    [SerializeField] private Image healthBarImage = null;

    [SerializeField] private float healthBarDisplayDelay = 5;

    private void Awake()
    {
        health.ClientOnHealthUpdated += HandleHealthUpdated;
    }

    private void OnDestroy()
    {
        health.ClientOnHealthUpdated -= HandleHealthUpdated;
    }

    private void OnMouseEnter()
    {
        healthBarParent.SetActive(true);
    }

    private void OnMouseExit()
    {
        healthBarParent.SetActive(false);
    }


    private void HandleHealthUpdated(int currentHealth, int maxHealth)
    {
        //Called when health is updated.
        healthBarImage.fillAmount = (float)currentHealth / maxHealth;
        StartCoroutine(HealthDisplayDelay(currentHealth, maxHealth));
    }

    IEnumerator HealthDisplayDelay(int currentHealth, int maxHealth)
    {
        if (healthBarParent.activeSelf) yield break;
        if (currentHealth == maxHealth) yield break;

        healthBarParent.SetActive(true);
        yield return new WaitForSeconds(healthBarDisplayDelay);
        healthBarParent.SetActive(false);
    }



}
