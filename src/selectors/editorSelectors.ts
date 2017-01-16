import { createSelector } from 'reselect'

import { getEditor as getState } from 'selectors'

export const getSource = createSelector(
  getState,
  state => state.source
)