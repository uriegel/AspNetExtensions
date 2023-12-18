import './App.css'
import { request, setBaseUrl, setMapFetchError } from './extensions'

setBaseUrl("http://localhost:2000/requests")

const matchError = (s: string) => s

setMapFetchError<string>(matchError)

type Request2 = {
	name: string
	id: number
}

type Result = {
	result: string
	id: number
}

type ErrorResult = {
	msg: string
	code: number
}

const makeRequest1Type = () => ({ method: "req1" })
const makeRequest2Type = (payload: Request2) => ({
    method: "req2",
	payload
})
const makeRequest3Type = () => ({ method: "req3"})

function App() {

	// TODO static mapping function to Error from Result
	// TODO no connection
	// TODO timeout
	// TODO cors
	// TODO wrong method/path

	const onRequest = async () => {
		const res = request<Result, ErrorResult>(makeRequest1Type())
		console.log(await res.toResult())
	}

	const onRequest2 = async () => {
		const res = request<Result, ErrorResult>(makeRequest2Type({ name: "Uwe Riegel", id: 9865 }))
		console.log(await res.toResult())
	}

	const onRequest3 = async () => {
		const res = request<Result, ErrorResult>(makeRequest3Type())
		console.log(await res.toResult())
	}
	
	const onRequest4 = async () => {
		setBaseUrl("http://localhost:2001/requests")
		const res = request<Result, ErrorResult>(makeRequest1Type())
		console.log(await res.toResult())
		setBaseUrl("http://localhost:2000/requests")
	}

	return (
		<div>
			<button onClick={onRequest}>Request</button>
			<button onClick={onRequest2}>Request 2</button>
			<button onClick={onRequest3}>Request Error</button>
			<button onClick={onRequest4}>No Connection</button>
		</div>
  	)
}

export default App
