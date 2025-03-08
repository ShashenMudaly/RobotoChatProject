import { API_BASE_URL } from "@/app/config/settings"

export async function POST(request: Request) {
  const body = await request.json()
  
  const response = await fetch(`${API_BASE_URL}/api/Chat`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body)
  })

  const data = await response.json()
  return Response.json(data)
}

