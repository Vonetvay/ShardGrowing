using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Altar : MonoBehaviour, IMailComponent
{
    [SerializeField] private LayerMask _packageLayer;
    private Collider2D _collider;
    private ContactFilter2D _filter;
    private readonly List<Collider2D> _results = new List<Collider2D>();

    private void Awake()
    {
        MailSystem.RegisterNewMail(this);
        
        _collider = GetComponent<Collider2D>();

        _filter = new ContactFilter2D();
        _filter.SetLayerMask(_packageLayer);
        _filter.useLayerMask = true;
        _filter.useTriggers = true;
    }

    public GameObject[] TakeRequest()
    {
        _collider.Overlap(_filter, _results);
        foreach (var res in _results) 
        {
            Destroy(res.gameObject);
        }
        return _results.Select(item => item.gameObject).ToArray();
    }

    public void GetRequest(GameObject[] objects)
    {
        foreach (var o in objects) 
        {
            Instantiate(o, transform.position, Quaternion.identity);
        }
    }
}
