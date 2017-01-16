import * as React from 'react'
import { connect } from 'react-redux'

import { ActionType } from 'actions'

import { State } from 'reducers'
import { buildActionCreators } from 'actions'

import Playground from 'components//Playground'

const mapStateToProps = (appState: State) => ({

})

const mapDispatchToProps = buildActionCreators({
})

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(Playground)