type RequestType = {
    method: string
    payload?: any
}

let baseUrl = ""

export const setBaseUrl = (url: string) => baseUrl = url

export async function request(request: RequestType) {
 
    const msg = {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request.payload)
    }

    const response = await fetch(`${baseUrl}/${request.method}`, msg) 
    return await response.text()
}