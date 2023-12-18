import { AsyncResult, Err, Ok, Result } from "functional-extensions"

type RequestType = {
    method: string
    /* eslint-disable @typescript-eslint/no-explicit-any */
    payload?: any
}

export interface ErrorType {
    status: number
    statusText: string
}

let baseUrl = ""

export const setBaseUrl = (url: string) => baseUrl = url

export function request<T, TE extends ErrorType>(request: RequestType): AsyncResult<T, TE> {
 
    const msg = {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request.payload)
    }

    const tryFetch = async (input: RequestInfo | URL, init?: RequestInit): Promise<Result<string, ErrorType>> => {
        try {
            const response = await fetch(input, init)
            return response.status == 200
                ? new Ok<string, ErrorType>(await response.text())
                : new Err<string, ErrorType>({
                    status: response.status + 1000,
                    statusText: response.statusText
                })
        } catch (err) {
            return new Err<string, ErrorType>({
                status: 0,
                /* eslint-disable @typescript-eslint/no-explicit-any */
                statusText: (err as any).message
            })
        }
    }

    const asyncFetch = (input: RequestInfo | URL, init?: RequestInit): AsyncResult<string, ErrorType> => 
        new AsyncResult<string, ErrorType>(tryFetch(input, init))
    
    return asyncFetch(`${baseUrl}/${request.method}`, msg)
        .mapError(e => e as TE)
        .bind(txt => Result.parseJSON<T, TE>(txt))
}