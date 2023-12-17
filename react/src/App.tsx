import './App.css'
import { request, setBaseUrl } from './extensions'

setBaseUrl("http://localhost:2000/requests")

type Request2 = {
	name: string
	id: number
}

const makeRequest1Type = () => ({ method: "req1"})

const makeRequest2Type = (payload: Request2) => ({
    method: "req2",
	payload
})

function App() {

	// TODO Result as result
	// TODO no connection
	// TODO timeout
	// TODO cors

	const onRequest = async () => {
		const res = await request(makeRequest1Type())
		console.log(res)
	}

	const onRequest2 = async () => {
		const res = await request(makeRequest2Type({ name: "Uwe Riegel", id: 9865 }))
		console.log(res)
	}

	return (
		<div>
			<button onClick={onRequest}>Request</button>
			<button onClick={onRequest2}>Request 2</button>
		</div>
  	)
}

export default App
