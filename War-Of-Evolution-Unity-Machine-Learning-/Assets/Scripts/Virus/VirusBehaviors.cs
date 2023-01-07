using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class VirusBehaviors : MonoBehaviour
{
    public Virus virus;
    
    public Material NormalCitizen;
    public Material InfectedCitizen;

    private MeshRenderer meshRenderer;
    private CitizenBehaviors citizenBehaviors;

    private CityManagement cityManagement;
    private CityPopulation cityPopulation;
    private CityVirusManagement cityVirusManagement;
    private CitizenSocialDistance citizenSocialDistance;

    private GameObject DieParticlePrefab;
    
    private void Awake()
    {
        DieParticlePrefab = Resources.Load("ParticleEffect/Die") as GameObject;
        cityVirusManagement = FindObjectOfType<CityVirusManagement>();
        cityVirusManagement.viruses.Add(this);
        cityPopulation = FindObjectOfType<CityPopulation>();
        cityManagement = FindObjectOfType<CityManagement>();
        citizenSocialDistance = GetComponent<CitizenSocialDistance>();
        citizenBehaviors = GetComponent<CitizenBehaviors>();
        meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        meshRenderer.material = NormalCitizen;
        virus = new Virus(1, 1, 1, 1);

        StartCoroutine(InfectOtherCitizens());
    }

    IEnumerator InfectOtherCitizens()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1f, 2f));
            if (citizenBehaviors.citizen.isVirus)
            {
                if (Random.Range(0, 100) <= 50f * virus.infectiousness)
                {
                    Collider[] near = Physics.OverlapSphere(transform.position,
                        transform.localScale.x * virus.infectionRadius * cityManagement.citizenInfectionDistance, LayerMask.GetMask("Citizen"));
                    if (near.Length > 0)
                    {
                        SetVirus(near, 0);
                    }
                }
            }
        }
    }


    public void GetInfected(Virus virus)
    {
        if(citizenBehaviors.citizen == null && (virus == null || citizenBehaviors.citizen == null || citizenBehaviors.citizen.isVirus)){return;}
        Inherited(virus);
        Debug.Log(transform.name + " got infected");
        meshRenderer.material = InfectedCitizen;
        citizenBehaviors.citizen.isVirus = true;
        cityPopulation.IncreaseInfected();
        StartCoroutine(Recovering());
        StartCoroutine(Evolve());
    }

    private void Inherited(Virus virus)
    {
        this.virus.resistance = virus.resistance;
        this.virus.infectionRadius = virus.infectionRadius;
        this.virus.virulence = virus.virulence;
    }

    public void GetCured()
    {
        meshRenderer.material = NormalCitizen;
        citizenBehaviors.citizen.isVirus = false;
        ResetVirus();
        cityPopulation.IncreaseCured();
    }
    
    private void ResetVirus()
    {
        virus.virulence = 1;
        virus.resistance = 1;
        virus.infectionRadius = 1;
    }

    public void GetDead()
    {
        cityPopulation.Citizens.Remove(gameObject);
        cityVirusManagement.viruses.Remove(this);
        cityPopulation.IncreaseDead();
        //Instantiate(DieParticlePrefab, null).transform.position = transform.position;
        Destroy(gameObject);
    }

    private void SetVirus(Collider[] virusArray, int index)
    {
        if (virusArray.Length > index)
        {
            if (virusArray[index].GetComponent<CitizenBehaviors>().citizen.isVirus == false)
            {
                virusArray[index].GetComponent<VirusBehaviors>().GetInfected(virus);
            }
            else
            {
                SetVirus(virusArray, index + 1);
            }
        }
    }

    IEnumerator Recovering()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 3f));
            if ((Random.Range(0, 50 * virus.resistance) / citizenBehaviors.citizen.immunity) <= 1f)
            {
                GetCured();
                yield break;
            }
            else if (Random.Range(0, 100) <= virus.virulence)
            {
                GetDead();
                yield break;
            }
        }
    }

    IEnumerator Evolve()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 6f));
            virus.resistance += 0.01f;
            virus.virulence += 0.06f;
            virus.infectionRadius += 0.06f;
            virus.infectiousness += 0.06f;
            if(citizenBehaviors.citizen.isVirus == false) { yield break; }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, transform.localScale.x * virus.infectionRadius * cityManagement.citizenInfectionDistance);
        }
    }
}