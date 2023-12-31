using UnityEngine;

[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor;

    public void Move(float delta)
    {
        Vector3 newPos = transform.localPosition;
        newPos.x -= delta * parallaxFactor;

        // Check if the layer is completely out of view on the left & move the layer to the right of all other layers
        if (newPos.x < -GetLayerWidth())
        {
            newPos.x += GetTotalWidth();
        }

        transform.localPosition = newPos;
    }

    float GetLayerWidth()
    {
        return GetComponent<Renderer>().bounds.size.x;
    }

    float GetTotalWidth()
    {
        float totalWidth = 0f;

        foreach (ParallaxLayer layer in FindObjectsOfType<ParallaxLayer>())
        {
            totalWidth += layer.GetLayerWidth();
        }

        return totalWidth;
    }
}


// Inspired by: https://www.youtube.com/watch?v=MEy-kIGE-lI