/* global devToolsExtension */
import { createStore, applyMiddleware, compose } from 'redux'
// import { createLogger } from 'redux-logger';
import createSagaMiddleware from 'redux-saga'

import { rootReducer } from 'reducers'
import saga from 'sagas/rootSaga'
import isProduction from 'helpers/isProduction'


export default function configureStore() {
  const sagaMiddleware = createSagaMiddleware()

  let enhancer
  if (isProduction) {
    enhancer = applyMiddleware(
      sagaMiddleware
    )
  } else {
    enhancer = applyMiddleware(
      sagaMiddleware,
      // createLogger()
    )
    const w = (window as any)
    if (w && w.devToolsExtension) {
      enhancer = compose(enhancer, w.devToolsExtension())
    }
  }
  const store = createStore(rootReducer, enhancer)
  sagaMiddleware.run(saga)

  const m = (module as any)
  if (!isProduction && m.hot) {
    m.hot.accept('./reducers/index', () => {
      // eslint-disable-next-line global-require
      store.replaceReducer(require('./reducers/index').default)
    })
  }
  return store
}