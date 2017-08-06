using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float sSensitivity = 100f;
	public float sensitivity = 100f;
	AtractorMulti.Particle targetParticle = null;
	Vector3 oldPos = Vector3.zero;
	// Use this for initialization
	Transform cam;
	bool playerMode = false;
	void Start()
	{
		cam = GetComponentInChildren<Camera>(true).transform;
	}
	Vector3 targetPos = Vector3.zero;
	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			playerMode = !playerMode;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			targetParticle = null;
		}
		if (Input.GetButton("Fire1"))
		{
			if (oldPos != Vector3.zero)
			{
				Vector3 del = (Input.mousePosition - oldPos);
				del.z = del.x;
				del.x = -del.y;
				del.y = del.z;
				del.z = 0f;
				transform.rotation *= Quaternion.Euler(del * Time.deltaTime * sensitivity);
			}
			oldPos = Input.mousePosition;
		}
		else
		{
			oldPos = Vector3.zero;
		}
		if (Input.GetButtonDown("Fire2"))
		{
			targetParticle = null;
			Ray r = cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
			Vector3 sta = r.origin;
			int count = 0;
			while (true)
			{
				for (int i = 0; i < AtractorMulti.Instance.parti.Length; i++)
				{
					if (Vector3.Distance(sta, AtractorMulti.Instance.parti[i].position) <= AtractorMulti.Instance.parti[i].size)
					{
						targetParticle = AtractorMulti.Instance.parti[i];
						break;
					}
				}
				sta += r.direction * 0.1f;
				count++;
				if (targetParticle != null || count > 10000)
					break;
			}
		}
		transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime);

		if (Input.mouseScrollDelta.sqrMagnitude > 0f)
		{
			cam.transform.position += cam.transform.forward * Input.mouseScrollDelta.y * Time.deltaTime * sSensitivity;
		}
		if (!playerMode)
		{
			if (AtractorMulti.Instance.Ready())
			{
				if (targetParticle != null)
				{
					if (targetParticle.alive)
					{
						targetPos = targetParticle.position;
					}
					else
					{
						targetParticle = null;
						targetPos = AtractorMulti.Instance.biggest.lPos;
					}
				}
				else
				{
					targetPos = AtractorMulti.Instance.biggest.lPos;
				}
			}
		}
		else
		{
			targetPos += transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * 5f;
			targetPos += transform.right * Input.GetAxis("Horizontal") * Time.deltaTime * 5f;
		}
	}
}
