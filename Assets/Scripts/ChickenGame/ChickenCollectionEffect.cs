using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChickenCollectionEffect : MonoBehaviour
{
    public static void Create(Vector3 position, Color chickenColor, int points)
    {
        GameObject effectParent = new GameObject("ChickenCollectionEffect");
        effectParent.transform.position = position;

        // floating text
        GameObject canvasObj = new GameObject("FloatingTextCanvas", typeof(RectTransform), typeof(Canvas));
        canvasObj.transform.SetParent(effectParent.transform);
        canvasObj.transform.localPosition = Vector3.up * 1.4f;
        
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform canvasRt = canvasObj.GetComponent<RectTransform>();
        canvasRt.sizeDelta = new Vector2(250, 100);
        canvasRt.localScale = Vector3.one * 0.007f;

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(canvasObj.transform, false);
        Text txt = textObj.GetComponent<Text>();
        
        try
        {
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch
        {
            try
            {
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch
            {
                Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
                if (fonts != null && fonts.Length > 0) txt.font = fonts[0];
            }
        }
        
        txt.text = "+" + points + " pkt";
        txt.fontSize = 28;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        
        Color textColor = chickenColor;
        if (chickenColor.r < 0.4f && chickenColor.g < 0.3f && chickenColor.b < 0.15f)
        {
            textColor = new Color(0.85f, 0.53f, 0.25f); 
        }
        txt.color = textColor;

        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(2f, -2f);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        canvasObj.AddComponent<FaceCamera>();

        for (int i = 0; i < 8; i++)
        {
            GameObject feather = GameObject.CreatePrimitive(PrimitiveType.Cube);
            feather.name = "FeatherPart";
            feather.transform.SetParent(effectParent.transform);
            feather.transform.localPosition = Random.insideUnitSphere * 0.2f;
            feather.transform.localScale = Vector3.one * Random.Range(0.08f, 0.18f);

            Renderer r = feather.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Legacy Shaders/Diffuse"));
            if (r.material.shader == null) r.material.shader = Shader.Find("Standard");
            r.material.color = Color.Lerp(chickenColor, Color.white, Random.Range(0f, 0.3f));
            
            Destroy(feather.GetComponent<Collider>()); 

            FeatherMotion fm = feather.AddComponent<FeatherMotion>();
            fm.velocity = new Vector3(
                Random.Range(-2.5f, 2.5f),
                Random.Range(2f, 5f),
                Random.Range(-2.5f, 2.5f)
            );
        }

        effectParent.AddComponent<CollectionEffectSelfDestruct>();
    }

    public static void CreateWheelEffect(Vector3 position, Color wheelColor, string effectText)
    {
        GameObject effectParent = new GameObject("WheelCollectionEffect");
        effectParent.transform.position = position;

        GameObject canvasObj = new GameObject("FloatingTextCanvas", typeof(RectTransform), typeof(Canvas));
        canvasObj.transform.SetParent(effectParent.transform);
        canvasObj.transform.localPosition = Vector3.up * 1.6f; 
        
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform canvasRt = canvasObj.GetComponent<RectTransform>();
        canvasRt.sizeDelta = new Vector2(350, 100);
        canvasRt.localScale = Vector3.one * 0.007f; 

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(canvasObj.transform, false);
        Text txt = textObj.GetComponent<Text>();
        
        try
        {
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch
        {
            try
            {
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch
            {
                Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
                if (fonts != null && fonts.Length > 0) txt.font = fonts[0];
            }
        }
        
        txt.text = effectText;
        txt.fontSize = 22;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = wheelColor;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(2f, -2f);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        canvasObj.AddComponent<FaceCamera>();

        for (int i = 0; i < 12; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.name = "WheelPart";
            particle.transform.SetParent(effectParent.transform);
            
            float angle = i * Mathf.PI * 2f / 12f;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * 0.8f, 0.1f, Mathf.Sin(angle) * 0.8f);
            particle.transform.localPosition = offset + Random.insideUnitSphere * 0.1f;
            particle.transform.localScale = Vector3.one * Random.Range(0.12f, 0.22f);

            Renderer r = particle.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Legacy Shaders/Diffuse"));
            if (r.material.shader == null) r.material.shader = Shader.Find("Standard");
            r.material.color = Color.Lerp(wheelColor, Color.white, Random.Range(0f, 0.3f));
            
            Destroy(particle.GetComponent<Collider>()); 

            FeatherMotion fm = particle.AddComponent<FeatherMotion>();
            fm.velocity = new Vector3(
                offset.x * 0.6f,
                Random.Range(3f, 5.5f),
                offset.z * 0.6f
            );
        }

        effectParent.AddComponent<CollectionEffectSelfDestruct>();
    }
}

public class FaceCamera : MonoBehaviour
{
    private Transform camTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (camTransform != null)
        {
            transform.LookAt(transform.position + camTransform.rotation * Vector3.forward, camTransform.rotation * Vector3.up);
        }
    }
}

public class FeatherMotion : MonoBehaviour
{
    public Vector3 velocity;
    private float gravity = -9.81f;
    private Vector3 rotationSpeed;

    private void Start()
    {
        rotationSpeed = new Vector3(
            Random.Range(-300f, 300f),
            Random.Range(-300f, 300f),
            Random.Range(-300f, 300f)
        );
    }

    private void Update()
    {
        velocity.y += gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        transform.Rotate(rotationSpeed * Time.deltaTime);

        transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime * 0.15f);
    }
}

public class CollectionEffectSelfDestruct : MonoBehaviour
{
    private float lifetime = 1.5f;
    private Vector3 floatSpeed = Vector3.up * 1.2f;

    private void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.position += floatSpeed * Time.deltaTime;
        }
    }
}
