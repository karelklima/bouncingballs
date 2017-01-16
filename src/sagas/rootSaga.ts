import { SagaIterator } from 'redux-saga';
import { fork } from 'redux-saga/effects';

function* defaultSaga(): SagaIterator {
  return true;
}

export default function* (): SagaIterator {
  yield [
    fork(defaultSaga)
  ];
}