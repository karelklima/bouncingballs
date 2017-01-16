import * as React from 'react'
import * as ReactDOM from 'react-dom'
import { Provider } from 'react-redux'

import configureStore from 'store'
import AppContainer from 'containers/AppContainer'

import 'reset.css'

const store = configureStore()

ReactDOM.render(
  <Provider store={store}>
    <AppContainer/>
  </Provider>,
  document.getElementById('root')
)
