import { connect } from 'react-redux'

import { State } from 'reducers'
import { buildActionCreators } from 'actions'

import { ActionType } from 'actions'

import Header from 'components/Header'

const mapStateToProps = (state: State) => ({
})

const mapDispatchToProps = buildActionCreators({
  onLogoClick: ActionType('TOGGLE_SETTINGS')
})

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(Header)