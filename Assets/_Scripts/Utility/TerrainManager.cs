using UnityEngine;
using System.Collections;

public class TerrainManager : MonoBehaviour
{
    private TerrainData m_terrainData;
    private float[,,] m_splatMapData;

    private int m_numTextures;

    // Use this for initialization
    void Start ()
    {
        m_terrainData = Terrain.activeTerrain.terrainData;

        int alphamapWidth = m_terrainData.alphamapWidth;
        int alphamapHeight = m_terrainData.alphamapHeight;

        m_splatMapData = m_terrainData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);
        m_numTextures = m_splatMapData.Length / (alphamapWidth * alphamapHeight);
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    private Vector3 ConvertToSplatMapCoordinate(Vector3 playerPos)
    {
        Vector3 vecRet = new Vector3();
        Terrain ter = Terrain.activeTerrain;
        Vector3 terPosition = ter.transform.position;
        vecRet.x = ((playerPos.x - terPosition.x) / ter.terrainData.size.x) * ter.terrainData.alphamapWidth;
        vecRet.z = ((playerPos.z - terPosition.z) / ter.terrainData.size.z) * ter.terrainData.alphamapHeight;
        return vecRet;
    }

    public int GetActiveTerrainTextureId(Vector3 pos)
    {        
        Vector3 TerrainCord = ConvertToSplatMapCoordinate(pos);
        int ret = 0;
        float comp = 0f;
        for (int i = 0; i < m_numTextures; i++)
        {
            if (comp < m_splatMapData[(int)TerrainCord.z, (int)TerrainCord.x, i])
                ret = i;
        }
        return ret;
    }
}
