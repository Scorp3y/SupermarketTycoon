using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject customerPrefab;
    public Transform spawnPoint;
    public Transform cashRegister;
    public Transform exitPoint;
    public List<ProductData> availableProducts;
    


    public float spawnInterval = 10f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnCustomer), 2f, spawnInterval);
    }

    void SpawnCustomer()
    {

        GameObject newCustomer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        Customer customer = newCustomer.GetComponent<Customer>();

        int productCount = Random.Range(1, 4);
        List<DesiredProduct> customerList = new List<DesiredProduct>(); 

        List<ProductData> copy = new List<ProductData>(availableProducts);
        for (int i = 0; i < productCount && copy.Count > 0; i++)
        {
            int index = Random.Range(0, copy.Count);
            ProductData chosen = copy[index];

            int quantity = Random.Range(1, 4);

            DesiredProduct dp = new DesiredProduct(chosen, quantity);
            customerList.Add(dp); 

            copy.RemoveAt(index);
        }
        customer.desiredProducts = customerList;
        customer.cashRegisterPoint = cashRegister;
        customer.exitPoint = exitPoint;
    }

}
