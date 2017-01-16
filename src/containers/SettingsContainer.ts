import { connect } from 'react-redux'

import { State } from 'reducers'
import { buildActionCreators } from 'actions'

import { getSource } from 'selectors/editorSelectors'

import Settings from 'components/Settings'

const mapStateToProps = (state: State) => ({
  source: getSource(state)
})

const mapDispatchToProps = buildActionCreators({})

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(Settings)