using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


[System.Serializable]
public class DesiredProduct
{
    public ProductData productData;
    public int quantity;

    public DesiredProduct(ProductData data, int qty)
    {
        productData = data;
        quantity = qty;
    }
}

public class Customer : MonoBehaviour
{
    public List<DesiredProduct> desiredProducts = new List<DesiredProduct>();
    public Transform cashRegisterPoint;
    public Transform exitPoint;

    public float waitTimeAtShelf = 2f;
    public float waitTimeAtCash = 3f;

    private NavMeshAgent agent;
    private int currentProductIndex = 0;
    private bool isShopping = true;

    private int totalSpent = 0;

    private bool isProcessing = false; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextProduct();
    }

    void GoToNextProduct()
    {
        if (currentProductIndex >= desiredProducts.Count)
        {
            isShopping = false;
            agent.SetDestination(cashRegisterPoint.position);
        }
        else
        {
            var desired = desiredProducts[currentProductIndex];
            agent.SetDestination(desired.productData.shelfPosition.position);
        }
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f && !isProcessing)
        {
            isProcessing = true; 

            if (isShopping)
                StartCoroutine(TakeProduct());
            else
                StartCoroutine(PayAndLeave());
        }
    }

    IEnumerator TakeProduct()
    {
        yield return new WaitForSeconds(waitTimeAtShelf);

        var desired = desiredProducts[currentProductIndex];
        bool success = WarehouseManager.Instance.TryTakeProduct(desired.productData.productName, desired.quantity);

        if (success)
        {
            totalSpent += desired.productData.price * desired.quantity;
            currentProductIndex++; 
        }
        else
        {
            currentProductIndex = desiredProducts.Count; 
            isShopping = false;
        }

        isProcessing = false;
        GoToNextProduct();
    }

    IEnumerator PayAndLeave()
    {
        yield return new WaitForSeconds(waitTimeAtCash);

        if (totalSpent > 0)
        {
            GameManager.Instance.AddMoney(totalSpent);
        }

        agent.SetDestination(exitPoint.position);

        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < 0.5f);

        Destroy(gameObject);
    }
}

